using System.Net;
using Apps.OpenLoc.Actions;
using Apps.OpenLoc.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
public class SegmentWriteTests
{
    private const string Timestamp = "2026-10-23T12:00:00.123456789Z";

    private static string State(bool confirm) => "{\"data\":{\"id\":9223372036854775807,\"target_text\":null,\"status\":\"" +
        (confirm ? "approved" : "needs_review") + "\",\"updated_at\":\"" + Timestamp + "\"}}";

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SegmentWrite_UsesCorrectMethodAndPreservesTextAndTimestamp(bool confirm)
    {
        var calls = 0;
        using var client = MockOpenLoc.Client(async request =>
        {
            calls++;
            Assert.AreEqual(confirm ? HttpMethod.Post : HttpMethod.Put, request.Method);
            Assert.AreEqual($"/api/v1/jobs/7/segments/20/{(confirm ? "confirm" : "draft")}", request.RequestUri!.AbsolutePath);
            Assert.AreEqual("Bearer test-key", request.Headers.Authorization!.ToString());
            var body = JObject.Parse(await request.Content!.ReadAsStringAsync(), new Newtonsoft.Json.Linq.JsonLoadSettings());
            Assert.AreEqual("  Translated text  ", body["text"]!.Value<string>());
            // Read the timestamp as plain JSON text so the assertion cannot round it.
            using var document = System.Text.Json.JsonDocument.Parse(await request.Content.ReadAsStringAsync());
            Assert.AreEqual(Timestamp, document.RootElement.GetProperty("expected_updated_at").GetString());
            Assert.AreEqual(2, document.RootElement.EnumerateObject().Count());
            return MockOpenLoc.Json(State(confirm));
        });
        var actions = new SegmentActions(new InvocationContext(), client);
        var input = new SegmentWriteRequest { JobId = "7", SegmentId = "20", TranslationText = "  Translated text  ", ExpectedUpdatedAt = Timestamp };
        var result = confirm ? await actions.ConfirmTranslation(input) : await actions.SaveDraftTranslation(input);
        Assert.AreEqual(1, calls);
        Assert.AreEqual("9223372036854775807", result.SegmentId);
        Assert.IsNull(result.TargetText);
        Assert.AreEqual(confirm ? "approved" : "needs_review", result.Status);
        Assert.AreEqual(Timestamp, result.UpdatedAt);
    }

    [DataTestMethod]
    [DataRow(false, null, null)]
    [DataRow(true, null, null)]
    [DataRow(false, "", " ")]
    [DataRow(true, "", "")]
    public async Task MissingOptionalInputs_SendEmptyObjectWithoutPlaceholders(bool confirm, string? text, string? timestamp)
    {
        using var client = MockOpenLoc.Client(async request =>
        {
            Assert.AreEqual("{}", await request.Content!.ReadAsStringAsync());
            return MockOpenLoc.Json(State(confirm));
        });
        var actions = new SegmentActions(new InvocationContext(), client);
        var input = new SegmentWriteRequest { JobId = "7", SegmentId = "20", TranslationText = text, ExpectedUpdatedAt = timestamp };
        if (confirm) await actions.ConfirmTranslation(input); else await actions.SaveDraftTranslation(input);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Conflict_DoesNotFetchNewTimestampOrRetry(bool confirm)
    {
        var calls = 0;
        using var client = MockOpenLoc.Client(_ =>
        {
            calls++;
            return Task.FromResult(MockOpenLoc.Json("""{"error":{"code":"conflict","message":"The segment changed since it was read"}}""", HttpStatusCode.Conflict));
        });
        var actions = new SegmentActions(new InvocationContext(), client);
        var input = new SegmentWriteRequest { JobId = "7", SegmentId = "20", ExpectedUpdatedAt = Timestamp };
        var error = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(async () =>
        {
            if (confirm) await actions.ConfirmTranslation(input); else await actions.SaveDraftTranslation(input);
        });
        StringAssert.Contains(error.Message, "review");
        StringAssert.Contains(error.Message, "segment changed");
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public async Task QaBlockedConfirmation_ShowsTypedErrorAndAllIssueFieldsWithoutBypass()
    {
        var calls = 0;
        using var client = MockOpenLoc.Client(_ =>
        {
            calls++;
            return Task.FromResult(MockOpenLoc.Json("""{"error":{"code":"QAErrorsBlocked","message":"Please fix quality errors","field":"text"},"qa_issues":[{"check_type":"tag_checks","severity":"error","message":"Missing closing tag","ignored":false},{"check_type":"terminology","severity":"warning","message":"Check preferred term","ignored":true}]}""", HttpStatusCode.UnprocessableEntity));
        });
        var error = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() =>
            new SegmentActions(new InvocationContext(), client).ConfirmTranslation(new() { JobId = "7", SegmentId = "20" }));
        foreach (var detail in new[] { "QAErrorsBlocked", "Please fix quality errors", "text", "tag_checks", "error", "Missing closing tag", "ignored: no", "terminology", "warning", "Check preferred term", "ignored: yes" })
            StringAssert.Contains(error.Message, detail);
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public async Task AddComment_UsesContentAndPreservesNullableAuthorsAndDistinctCommentId()
    {
        using var client = MockOpenLoc.Client(async request =>
        {
            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual("/api/v1/jobs/7/segments/20/comments", request.RequestUri!.AbsolutePath);
            var body = JObject.Parse(await request.Content!.ReadAsStringAsync());
            Assert.AreEqual(1, body.Count);
            Assert.AreEqual(" Review terminology ", body["content"]!.Value<string>());
            return MockOpenLoc.Json("""{"data":{"id":600,"content":" Review terminology ","author_id":null,"author_name":null,"created_at":"2026-10-23T12:00:00Z"}}""", HttpStatusCode.Created);
        });
        var result = await new SegmentActions(new InvocationContext(), client).AddSegmentComment(
            new() { JobId = "7", SegmentId = "20", Comment = " Review terminology " });
        Assert.AreEqual("600", result.CommentId);
        Assert.AreEqual(" Review terminology ", result.Content);
        Assert.IsNull(result.AuthorId);
        Assert.IsNull(result.AuthorName);
        Assert.AreEqual(2026, result.CreatedAt.Year);
    }

    [TestMethod]
    public async Task InvalidCommentAndTimestamp_FailBeforeAnyHttpRequest()
    {
        using var client = MockOpenLoc.Client(_ => throw new AssertFailedException("No request is expected for invalid input."));
        var actions = new SegmentActions(new InvocationContext(), client);
        foreach (var comment in new[] { "", " ", new string('x', 5001) })
            await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => actions.AddSegmentComment(
                new() { JobId = "7", SegmentId = "20", Comment = comment }));
        foreach (var timestamp in new[] { "yesterday", "2026-10-23T12:00:00", "2026-02-30T12:00:00Z" })
            await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => actions.SaveDraftTranslation(
                new() { JobId = "7", SegmentId = "20", ExpectedUpdatedAt = timestamp }));
    }

    [TestMethod]
    public async Task Comment_AllowsExactly5000Characters()
    {
        using var client = MockOpenLoc.Client(async request =>
        {
            Assert.AreEqual(5000, JObject.Parse(await request.Content!.ReadAsStringAsync())["content"]!.Value<string>()!.Length);
            return MockOpenLoc.Json("""{"data":{"id":600,"content":"Saved","author_id":1,"author_name":"Reviewer","created_at":"2026-10-23T12:00:00Z"}}""", HttpStatusCode.Created);
        });
        var result = await new SegmentActions(new InvocationContext(), client).AddSegmentComment(
            new() { JobId = "7", SegmentId = "20", Comment = new string('x', 5000) });
        Assert.AreEqual("1", result.AuthorId);
        Assert.AreEqual("Reviewer", result.AuthorName);
    }

    [DataTestMethod]
    [DataRow(400)]
    [DataRow(401)]
    [DataRow(403)]
    [DataRow(404)]
    [DataRow(429)]
    public async Task SegmentWritesAndComments_SurfaceDocumentedErrorsWithoutRetries(int status)
    {
        var calls = 0;
        using var client = MockOpenLoc.Client(_ =>
        {
            calls++;
            return Task.FromResult(MockOpenLoc.Json("""{"error":{"code":"failure","message":"API explanation"}}""", (HttpStatusCode)status));
        });
        var actions = new SegmentActions(new InvocationContext(), client);
        foreach (var operation in new Func<Task>[]
        {
            async () => { await actions.SaveDraftTranslation(new() { JobId = "7", SegmentId = "20" }); },
            async () => { await actions.ConfirmTranslation(new() { JobId = "7", SegmentId = "20" }); },
            async () => { await actions.AddSegmentComment(new() { JobId = "7", SegmentId = "20", Comment = "Review" }); }
        })
        {
            Exception? failure = null;
            try { await operation(); }
            catch (Exception ex) when (ex is PluginMisconfigurationException or PluginApplicationException) { failure = ex; }
            Assert.IsNotNull(failure);
            StringAssert.Contains(failure.Message, "API explanation");
            Assert.AreEqual(status == 429, failure is PluginApplicationException);
        }
        Assert.AreEqual(3, calls);
    }
}
