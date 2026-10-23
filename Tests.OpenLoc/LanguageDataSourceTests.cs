using Apps.OpenLoc.Handlers.Static;

namespace Tests.OpenLoc;

[TestClass]
public class LanguageDataSourceTests
{
    [TestMethod]
    public void Languages_UseOnlyDocumentedCodesAndReadableNames()
    {
        var items = new LanguageDataSourceHandler().GetData().ToList();
        CollectionAssert.AreEquivalent(new[] { "en", "de", "fr" }, items.Select(x => x.Value).ToArray());
        Assert.AreEqual(items.Count, items.Select(x => x.Value).Distinct().Count());
        Assert.IsTrue(items.All(x => !string.IsNullOrWhiteSpace(x.DisplayName) && x.DisplayName != x.Value));
    }
}
