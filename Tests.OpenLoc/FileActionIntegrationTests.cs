using System.Text;
using Apps.OpenLoc.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
[TestCategory("Integration")]
public class FileActionIntegrationTests : TestBase
{
    [TestMethod]
    [TestCategory("CreatesData")]
    public async Task UploadFile_CreatesDocumentAndReturnsFileId()
    {
        if (!TestData.TryGetValue("enable_upload_file", out var enabled) ||
            !bool.TryParse(enabled, out var allowed) || !allowed)
            Assert.Inconclusive("Set TestData.enable_upload_file to true to upload a disposable file and create its jobs.");
        var bytes = Encoding.UTF8.GetBytes("<!doctype html><html><body><p>Hello from the file integration test.</p></body></html>");
        var manager = new InMemoryFileManager(bytes);
        var result = await new FileActions(InvocationContext, manager).UploadFile(new()
        {
            ProjectId = RequireTestData("project_id"),
            File = new FileReference { Name = $"integration-{Guid.NewGuid():N}.html", ContentType = "text/html" },
            Analyze = false
        });
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.FileId));
        Assert.AreEqual(RequireTestData("project_id"), result.ProjectId);
        Assert.IsNotNull(result.Warnings);
        Console.WriteLine($"Created file {result.FileId}. Remove this disposable file and its automatically created jobs manually.");
    }

    [DataTestMethod]
    [DataRow("source")]
    [DataRow("xliff")]
    public async Task DownloadTranslatedFile_ReturnsUsableNativeFile(string format)
    {
        var manager = new InMemoryFileManager();
        var result = await new FileActions(InvocationContext, manager).DownloadTranslatedFile(new()
        {
            JobId = RequireTestData("job_id"), ExportFormat = format
        });
        Assert.AreSame(manager.StoredFile, result.File);
        Assert.IsTrue(manager.StoredBytes is { Length: > 0 });
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.File.Name));
    }
}
