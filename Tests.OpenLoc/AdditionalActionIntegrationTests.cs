using Apps.OpenLoc.Actions;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
[TestCategory("Integration")]
public class AdditionalActionIntegrationTests : TestBase
{
    private void RequireOptIn(string key)
    {
        if (!TestData.TryGetValue(key, out var text) || !bool.TryParse(text, out var enabled) || !enabled)
            Assert.Inconclusive($"Set TestData.{key} to true only for a disposable test segment.");
    }

    [TestMethod]
    [TestCategory("UpdatesData")]
    public async Task SaveDraftTranslation_ReturnsNeedsReview()
    {
        RequireOptIn("enable_save_segment_draft");
        var result = await new SegmentActions(InvocationContext).SaveDraftTranslation(new()
        {
            JobId = RequireTestData("job_id"), SegmentId = RequireTestData("segment_id"),
            TranslationText = RequireTestData("translation_text")
        });
        Assert.AreEqual("needs_review", result.Status);
        Assert.AreEqual(RequireTestData("translation_text"), result.TargetText);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.UpdatedAt));
    }

    [TestMethod]
    [TestCategory("UpdatesData")]
    public async Task ConfirmTranslation_ReturnsApproved()
    {
        RequireOptIn("enable_confirm_segment");
        var result = await new SegmentActions(InvocationContext).ConfirmTranslation(new()
        {
            JobId = RequireTestData("job_id"), SegmentId = RequireTestData("segment_id"),
            TranslationText = RequireTestData("translation_text")
        });
        Assert.AreEqual("approved", result.Status);
        Assert.AreEqual(RequireTestData("translation_text"), result.TargetText);
    }

    [TestMethod]
    [TestCategory("CreatesData")]
    public async Task AddSegmentComment_ReturnsNewComment()
    {
        RequireOptIn("enable_add_segment_comment");
        var content = $"Disposable integration comment {Guid.NewGuid():N}";
        var result = await new SegmentActions(InvocationContext).AddSegmentComment(new()
        {
            JobId = RequireTestData("job_id"), SegmentId = RequireTestData("segment_id"), Comment = content
        });
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.CommentId));
        Assert.AreEqual(content, result.Content);
        Console.WriteLine($"Created comment {result.CommentId}; remove it manually when finished.");
    }

    [TestMethod]
    public async Task DownloadOriginalFile_ReturnsNativeFile()
    {
        var manager = new InMemoryFileManager();
        var result = await new FileActions(InvocationContext, manager).DownloadOriginalFile(new()
        {
            FileId = RequireTestData("file_id")
        });
        Assert.AreSame(manager.StoredFile, result.File);
        Assert.IsTrue(manager.StoredBytes is { Length: > 0 });
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.File.Name));
    }
}
