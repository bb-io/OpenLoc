using Apps.OpenLoc.Actions;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
[TestCategory("Integration")]
public class SegmentActionTests : TestBase
{
    [TestMethod]
    public async Task SearchJobSegments_ReturnsDocumentOrderAndText()
    {
        var result = await new SegmentActions(InvocationContext).SearchJobSegments(new()
        {
            JobId = RequireTestData("job_id")
        });
        Assert.IsNotNull(result.Segments);
        Assert.IsTrue(result.Segments.Count > 0, "Choose a processed job containing segments.");
        Assert.IsTrue(result.Segments.All(x => !string.IsNullOrWhiteSpace(x.SegmentId)));
        Assert.IsTrue(result.Segments.All(x => x.SourceText is not null));
        CollectionAssert.AreEqual(result.Segments.Select(x => x.Position).OrderBy(x => x).ToList(),
            result.Segments.Select(x => x.Position).ToList());
    }
}
