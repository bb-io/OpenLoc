using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Apps.OpenLoc.Actions;
using Apps.OpenLoc.Api;
using Apps.OpenLoc.Constants;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
public class FileTransferTests
{
    private const string StoredFileJson = """{"data":{"id":9223372036854775807,"project_id":42,"name":"sample.html","format":"html","segment_count":2,"created_at":"2026-10-23T12:00:00Z"},"warnings":["Analysis could not be started"]}""";

    private static OpenLocClient Client(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) => new(
    [
        new AuthenticationCredentialsProvider(CredsNames.Hostname, "https://example.org"),
        new AuthenticationCredentialsProvider(CredsNames.AccessKey, "test-key")
    ], new ResponseHandler(respond));

    private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    [TestMethod]
    public async Task Upload_UsesNativeForwardOnlyFileAndMultipartWithWarnings()
    {
        var bytes = Encoding.UTF8.GetBytes("<p>Hello</p>");
        var calls = 0;
        using var client = Client(async request =>
        {
            calls++;
            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual("/api/v1/projects/42/files", request.RequestUri!.AbsolutePath);
            Assert.AreEqual("Bearer test-key", request.Headers.Authorization!.ToString());
            Assert.AreEqual("multipart/form-data", request.Content!.Headers.ContentType!.MediaType);
            Assert.IsTrue(request.Content.Headers.ContentType.Parameters.Any(x => x.Name == "boundary" && !string.IsNullOrEmpty(x.Value)));
            var parts = ((MultipartFormDataContent)request.Content).ToList();
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("file", parts[0].Headers.ContentDisposition!.Name!.Trim('"'));
            Assert.AreEqual("sample.html", parts[0].Headers.ContentDisposition!.FileName!.Trim('"'));
            CollectionAssert.AreEqual(bytes, await parts[0].ReadAsByteArrayAsync());
            return Json(StoredFileJson, HttpStatusCode.Created);
        });
        var actions = new FileActions(new InvocationContext(), new InMemoryFileManager(bytes), client);
        var result = await actions.UploadFile(new()
        {
            ProjectId = "42", File = new FileReference { Name = "sample.html", ContentType = "text/html" }
        });
        Assert.AreEqual(1, calls, "Uploading must not make additional jobs or repeat the POST.");
        Assert.AreEqual("9223372036854775807", result.FileId);
        Assert.AreEqual("42", result.ProjectId);
        Assert.AreEqual("html", result.FileFormat);
        Assert.AreEqual(2, result.SegmentCount);
        CollectionAssert.AreEqual(new[] { "Analysis could not be started" }, result.Warnings);
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Upload_OptionalFieldsAreFormTextIncludingExplicitFalse(bool analyze)
    {
        const string layout = """{"sheets":[{"name":"Sheet1","columns":["B"],"header_row":true}]}""";
        using var client = Client(async request =>
        {
            var parts = ((MultipartFormDataContent)request.Content!).ToDictionary(x => x.Headers.ContentDisposition!.Name!.Trim('"'));
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual(analyze ? "true" : "false", await parts["analyze"].ReadAsStringAsync());
            Assert.AreEqual(layout, await parts["xlsx_layout"].ReadAsStringAsync());
            return Json(StoredFileJson, HttpStatusCode.Created);
        });
        await client.UploadFileAsync("42", [1, 2, 3], "sample.xlsx", analyze: analyze, xlsxLayout: layout);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("source")]
    [DataRow("xliff")]
    public async Task Download_ReturnsNativeFileWithBinaryContentAndResponseFilename(string? format)
    {
        byte[] bytes = [0, 255, 128, 1, 2, 3];
        var fileName = format == "xliff" ? "translated.xlf" : "translated.docx";
        var contentType = format == "xliff" ? "application/x-xliff+xml" : "application/octet-stream";
        using var client = Client(request =>
        {
            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual("/api/v1/jobs/7/export", request.RequestUri!.AbsolutePath);
            Assert.AreEqual(format is null ? "" : "?format=" + format, request.RequestUri.Query);
            Assert.AreEqual("Bearer test-key", request.Headers.Authorization!.ToString());
            StringAssert.Contains(request.Headers.Accept.ToString(), contentType);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = fileName };
            return Task.FromResult(response);
        });
        var manager = new InMemoryFileManager();
        var result = await new FileActions(new InvocationContext(), manager, client).DownloadTranslatedFile(
            new() { ProjectId = "42", JobId = "7", ExportFormat = format });
        Assert.AreSame(manager.StoredFile, result.File);
        Assert.AreEqual(fileName, result.File.Name);
        Assert.AreEqual(contentType, result.File.ContentType);
        CollectionAssert.AreEqual(bytes, manager.StoredBytes);
    }

    [DataTestMethod]
    [DataRow(null, "brief.docx")]
    [DataRow("xliff", "brief.xlf")]
    public async Task Download_WithoutDispositionDerivesFilenameFromJob(string? format, string expected)
    {
        using var client = Client(request => Task.FromResult(request.RequestUri!.AbsolutePath.EndsWith("/export")
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) }
            : Json("""{"data":{"id":7,"file_name":"brief.docx","target_language":"de"}}""")));
        var manager = new InMemoryFileManager();
        var result = await new FileActions(new InvocationContext(), manager, client).DownloadTranslatedFile(new() { JobId = "7", ExportFormat = format });
        Assert.AreEqual(expected, result.File.Name);
    }

    [DataTestMethod]
    [DataRow(400)]
    [DataRow(401)]
    [DataRow(403)]
    [DataRow(404)]
    [DataRow(429)]
    public async Task FileRequests_SurfaceApiErrorsAndNeverRetryUploads(int status)
    {
        var calls = 0;
        using var client = Client(_ =>
        {
            calls++;
            var response = Json("""{"error":{"code":"test_error","message":"Detailed API failure","field":"file"}}""", (HttpStatusCode)status);
            response.Headers.TryAddWithoutValidation("Retry-After", "0");
            return Task.FromResult(response);
        });
        try
        {
            await client.UploadFileAsync("42", [1], "sample.html");
            Assert.Fail("Upload should report the failure.");
        }
        catch (Exception ex) when (ex is PluginMisconfigurationException or PluginApplicationException)
        {
            StringAssert.Contains(ex.Message, "Detailed API failure");
            Assert.AreEqual(status == 429, ex is PluginApplicationException);
        }
        Assert.AreEqual(1, calls);
        try
        {
            await client.DownloadTranslatedFileAsync("7");
            Assert.Fail("Download should report the failure.");
        }
        catch (Exception ex) when (ex is PluginMisconfigurationException or PluginApplicationException)
        {
            StringAssert.Contains(ex.Message, "Detailed API failure");
            Assert.AreEqual(status == 429, ex is PluginApplicationException);
        }
    }

    [TestMethod]
    public async Task Download_RejectsEmptyBodyAndSuccessfulJsonInsteadOfFile()
    {
        using var empty = Client(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([]) }));
        await Assert.ThrowsExceptionAsync<PluginApplicationException>(() => empty.DownloadTranslatedFileAsync("7"));
        using var json = Client(_ => Task.FromResult(Json("""{"error":{"message":"unexpected"}}""")));
        await Assert.ThrowsExceptionAsync<PluginApplicationException>(() => json.DownloadTranslatedFileAsync("7"));
    }

    [TestMethod]
    public void Filename_PrefersUtf8DispositionAndStripsDirectories()
    {
        var response = new RestResponse { Headers = [new HeaderParameter("Content-Disposition", "attachment; filename=backup.xlf; filename*=UTF-8''traduction%20fran%C3%A7aise.xlf")] };
        Assert.AreEqual("traduction française.xlf", FileTransfer.ResponseFileName(response));
        Assert.AreEqual("safe.docx", FileTransfer.SafeFileName("../../safe.docx"));
    }

    [TestMethod]
    public void UploadSize_AcceptsLimitAndRejectsEmptyOrOversizedFiles()
    {
        FileTransfer.ValidateUpload(FileTransfer.MaxUploadBytes);
        Assert.ThrowsException<PluginMisconfigurationException>(() => FileTransfer.ValidateUpload(FileTransfer.MaxUploadBytes + 1));
        Assert.ThrowsException<PluginMisconfigurationException>(() => FileTransfer.ValidateUpload(0));
    }

    private sealed class ResponseHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }
}
