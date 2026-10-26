using System.IO.Compression;
using System.Text;
using Apps.OpenLoc.Actions;
using Apps.OpenLoc.Api;
using Apps.OpenLoc.Constants;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
public class XlsxLayoutValidationTests
{
    private const string Layout = """{"sheets":[{"name":"Sheet1","columns":["B"],"header_row":true}]}""";

    [DataTestMethod]
    [DataRow("en")]
    [DataRow("de")]
    [DataRow("fr")]
    public void Layout_RejectsWorkbooksWithProjectLanguageCodesInFirstRow(string code)
    {
        var bytes = Workbook(code, sharedString: false);
        var error = Assert.ThrowsException<PluginMisconfigurationException>(() =>
            XlsxLayoutValidation.ValidateHeaders(bytes, ["en", "de", "fr"]));
        StringAssert.Contains(error.Message, "row 1");
        StringAssert.Contains(error.Message, "empty");
    }

    [TestMethod]
    public void Layout_DetectsSharedStringLanguageHeadersAndAllowsOrdinaryHeaders()
    {
        Assert.ThrowsException<PluginMisconfigurationException>(() =>
            XlsxLayoutValidation.ValidateHeaders(Workbook("de", sharedString: true), ["en", "de"]));
        XlsxLayoutValidation.ValidateHeaders(Workbook("Heading", sharedString: true), ["en", "de"]);
        XlsxLayoutValidation.ValidateJson(Layout, "sample.xlsx");
        XlsxLayoutValidation.ValidateJson(null, "sample.xlsx");
    }

    [DataTestMethod]
    [DataRow("not json")]
    [DataRow("{}")]
    [DataRow("{\"sheets\":[{}]}")]
    [DataRow("{\"sheets\":[{\"name\":\"Sheet1\",\"columns\":[1]}]}")]
    [DataRow("{\"sheets\":[{\"name\":\"Sheet1\",\"header_row\":\"true\"}]}")]
    public void Layout_RejectsIncorrectJsonShape(string json)
    {
        Assert.ThrowsException<PluginMisconfigurationException>(() => XlsxLayoutValidation.ValidateJson(json, "sample.xlsx"));
    }

    [TestMethod]
    public void Layout_RejectsNonXlsxFilesAndUnreadableWorkbooks()
    {
        Assert.ThrowsException<PluginMisconfigurationException>(() => XlsxLayoutValidation.ValidateJson(Layout, "sample.docx"));
        Assert.ThrowsException<PluginMisconfigurationException>(() => XlsxLayoutValidation.ValidateHeaders([1, 2, 3], ["en"]));
    }

    [DataTestMethod]
    [DataRow("de", false)]
    [DataRow("Heading", true)]
    public async Task Upload_ChecksWorkbookBeforeSendingLayout(string header, bool shouldUpload)
    {
        var posts = 0;
        using var client = new OpenLocClient(
        [
            new AuthenticationCredentialsProvider(CredsNames.Hostname, "https://example.org"),
            new AuthenticationCredentialsProvider(CredsNames.AccessKey, "test-key")
        ], new WorkbookHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                Assert.AreEqual("/api/v1/projects/42", request.RequestUri!.AbsolutePath);
                return new(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"data":{"id":42,"source_language":"en","target_languages":["de"]}}""", Encoding.UTF8, "application/json")
                };
            }
            posts++;
            Assert.IsTrue(shouldUpload, "A workbook with language headers must not be submitted with a layout.");
            return new(System.Net.HttpStatusCode.Created)
            {
                Content = new StringContent("""{"data":{"id":10,"project_id":42,"name":"sample.xlsx","format":"xlsx"}}""", Encoding.UTF8, "application/json")
            };
        }));
        var actions = new FileActions(new InvocationContext(), new InMemoryFileManager(Workbook(header, false)), client);
        var request = new Apps.OpenLoc.Models.Requests.UploadFileRequest
        {
            ProjectId = "42", File = new FileReference { Name = "sample.xlsx" }, XlsxLayout = Layout
        };
        if (shouldUpload)
        {
            var result = await actions.UploadFile(request);
            Assert.AreEqual("10", result.FileId);
            Assert.AreEqual(0, result.Warnings.Count);
        }
        else await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => actions.UploadFile(request));
        Assert.AreEqual(shouldUpload ? 1 : 0, posts);
    }

    private sealed class WorkbookHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(respond(request));
    }

    private static byte[] Workbook(string header, bool sharedString)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            using (var writer = new StreamWriter(archive.CreateEntry("xl/worksheets/sheet1.xml").Open(), Encoding.UTF8))
                writer.Write($"<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData><row r=\"1\"><c r=\"A1\" t=\"{(sharedString ? "s" : "inlineStr")}\">{(sharedString ? "<v>0</v>" : $"<is><t>{header}</t></is>")}</c></row></sheetData></worksheet>");
            if (sharedString)
            {
                using var writer = new StreamWriter(archive.CreateEntry("xl/sharedStrings.xml").Open(), Encoding.UTF8);
                writer.Write($"<sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><si><t>{header}</t></si></sst>");
            }
        }
        return stream.ToArray();
    }
}
