using System.Net;
using System.Net.Http.Headers;
using Apps.OpenLoc.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
public class OriginalFileDownloadTests
{
    private static readonly byte[] Bytes = [0, 255, 128, 1, 7, 9];

    private static HttpResponseMessage Binary(string? name = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Bytes) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        if (name is not null) response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = name };
        return response;
    }

    [TestMethod]
    public async Task OriginalDownload_UsesOnlyFileIdAndPreservesBinaryAndFilename()
    {
        var calls = 0;
        using var client = MockOpenLoc.Client(request =>
        {
            calls++;
            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual("/api/v1/files/5/source", request.RequestUri!.AbsolutePath);
            Assert.AreEqual("", request.RequestUri.Query);
            Assert.AreEqual("Bearer test-key", request.Headers.Authorization!.ToString());
            StringAssert.Contains(request.Headers.Accept.ToString(), "application/octet-stream");
            return Task.FromResult(Binary("uploaded.docx"));
        });
        var manager = new InMemoryFileManager();
        var result = await new FileActions(new InvocationContext(), manager, client).DownloadOriginalFile(new() { FileId = "5", ProjectId = "42" });
        Assert.AreEqual(1, calls);
        Assert.AreSame(manager.StoredFile, result.File);
        Assert.AreEqual("uploaded.docx", result.File.Name);
        Assert.AreEqual("application/octet-stream", result.File.ContentType);
        CollectionAssert.AreEqual(Bytes, manager.StoredBytes);
    }

    [DataTestMethod]
    [DataRow("uploaded.docx", "uploaded.docx")]
    [DataRow(null, "renamed.docx")]
    public async Task MissingFilename_UsesOriginalNameBeforeCurrentName(string? originalName, string expected)
    {
        using var client = MockOpenLoc.Client(request =>
        {
            if (request.RequestUri!.AbsolutePath == "/api/v1/files/5/source") return Task.FromResult(Binary());
            Assert.AreEqual("/api/v1/projects/42/files/5", request.RequestUri.AbsolutePath);
            return Task.FromResult(MockOpenLoc.Json(new Newtonsoft.Json.Linq.JObject
            {
                ["data"] = new Newtonsoft.Json.Linq.JObject { ["id"] = 5, ["name"] = "renamed.docx", ["original_name"] = originalName }
            }.ToString()));
        });
        var manager = new InMemoryFileManager();
        var result = await new FileActions(new InvocationContext(), manager, client).DownloadOriginalFile(new() { FileId = "5", ProjectId = "42" });
        Assert.AreEqual(expected, result.File.Name);
        CollectionAssert.AreEqual(Bytes, manager.StoredBytes);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("42")]
    public async Task MissingOrForbiddenMetadata_DoesNotBlockAuthorizedLinguistDownload(string? projectId)
    {
        var calls = 0;
        using var client = MockOpenLoc.Client(request =>
        {
            calls++;
            return Task.FromResult(request.RequestUri!.AbsolutePath.EndsWith("/source") ? Binary()
                : MockOpenLoc.Json("""{"error":{"code":"forbidden","message":"Metadata requires project manager access"}}""", HttpStatusCode.Forbidden));
        });
        var manager = new InMemoryFileManager();
        var result = await new FileActions(new InvocationContext(), manager, client).DownloadOriginalFile(new() { FileId = "5", ProjectId = projectId });
        Assert.AreEqual("original-file-5.bin", result.File.Name);
        Assert.AreEqual(projectId is null ? 1 : 2, calls);
        CollectionAssert.AreEqual(Bytes, manager.StoredBytes);
    }

    [DataTestMethod]
    [DataRow(401)]
    [DataRow(403)]
    [DataRow(404)]
    [DataRow(429)]
    public async Task OriginalDownload_ReportsOnlyDocumentedErrorsWithoutStoringFile(int status)
    {
        using var client = MockOpenLoc.Client(_ => Task.FromResult(MockOpenLoc.Json(
            """{"error":{"code":"failure","message":"Original download denied"}}""", (HttpStatusCode)status)));
        var manager = new InMemoryFileManager();
        Exception? failure = null;
        try { await new FileActions(new InvocationContext(), manager, client).DownloadOriginalFile(new() { FileId = "5" }); }
        catch (Exception ex) when (ex is PluginMisconfigurationException or PluginApplicationException) { failure = ex; }
        Assert.IsNotNull(failure);
        StringAssert.Contains(failure.Message, "Original download denied");
        Assert.AreEqual(status == 429, failure is PluginApplicationException);
        Assert.IsNull(manager.StoredFile);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task EmptyOrJsonDownload_FailsBeforeStorage(bool json)
    {
        using var client = MockOpenLoc.Client(_ => Task.FromResult(json ? MockOpenLoc.Json("{}")
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([]) }));
        var manager = new InMemoryFileManager();
        await Assert.ThrowsExceptionAsync<PluginApplicationException>(() =>
            new FileActions(new InvocationContext(), manager, client).DownloadOriginalFile(new() { FileId = "5" }));
        Assert.IsNull(manager.StoredFile);
    }
}
