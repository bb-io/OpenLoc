using Apps.OpenLoc.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
public class DependentDataSourceTests
{
    [TestMethod]
    public async Task SegmentDropdown_UsesSelectedJobAndServerSearchAcrossPages()
    {
        var pages = 0;
        using var client = MockOpenLoc.Client(request =>
        {
            Assert.AreEqual("/api/v1/jobs/7/segments", request.RequestUri!.AbsolutePath);
            StringAssert.Contains(request.RequestUri.Query, "q=hello");
            StringAssert.Contains(request.RequestUri.Query, "limit=100");
            pages++;
            if (pages == 1)
            {
                Assert.IsFalse(request.RequestUri.Query.Contains("cursor="));
                return Task.FromResult(MockOpenLoc.Json("""{"data":[{"id":20,"source_text":"Hello world"}],"cursor":20,"has_more":true}"""));
            }
            StringAssert.Contains(request.RequestUri.Query, "cursor=20");
            return Task.FromResult(MockOpenLoc.Json("""{"data":[{"id":21,"source_text":"Hello again"}],"cursor":0,"has_more":false}"""));
        });
        var handler = new SegmentDataSourceHandler(new InvocationContext(), new() { JobId = "7" }, client);
        var items = (await handler.GetDataAsync(new() { SearchString = "hello" }, CancellationToken.None)).ToList();
        CollectionAssert.AreEqual(new[] { "20", "21" }, items.Select(x => x.Value).ToArray());
        StringAssert.Contains(items[0].DisplayName, "Hello world");
        Assert.AreEqual(2, pages);
    }

    [TestMethod]
    public async Task FileDropdown_UsesSelectedProjectAndFiltersAcrossPages()
    {
        var pages = 0;
        using var client = MockOpenLoc.Client(request =>
        {
            Assert.AreEqual("/api/v1/projects/42/files", request.RequestUri!.AbsolutePath);
            Assert.IsFalse(request.RequestUri.Query.Contains("q="), "The file list has no documented server-side search.");
            StringAssert.Contains(request.RequestUri.Query, "limit=100");
            pages++;
            if (pages == 1) return Task.FromResult(MockOpenLoc.Json("""{"data":[{"id":50,"name":"other.docx"}],"cursor":50,"has_more":true}"""));
            StringAssert.Contains(request.RequestUri.Query, "cursor=50");
            return Task.FromResult(MockOpenLoc.Json("""{"data":[{"id":51,"name":"Brief.docx"}],"cursor":0,"has_more":false}"""));
        });
        var handler = new FileDataSourceHandler(new InvocationContext(), new() { ProjectId = "42" }, client);
        var items = (await handler.GetDataAsync(new() { SearchString = "BRIEF" }, CancellationToken.None)).ToList();
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual("51", items[0].Value);
        StringAssert.Contains(items[0].DisplayName, "Brief.docx");
        Assert.AreEqual(2, pages);
    }

    [TestMethod]
    public async Task DependentDropdowns_WithoutParentReturnEmptyWithoutApiCalls()
    {
        using var client = MockOpenLoc.Client(_ => throw new AssertFailedException("No parent was selected."));
        var segments = await new SegmentDataSourceHandler(new InvocationContext(), new(), client).GetDataAsync(new(), CancellationToken.None);
        var files = await new FileDataSourceHandler(new InvocationContext(), new(), client).GetDataAsync(new(), CancellationToken.None);
        Assert.IsFalse(segments.Any());
        Assert.IsFalse(files.Any());
    }
}
