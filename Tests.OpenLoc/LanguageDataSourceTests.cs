using Apps.OpenLoc.Handlers.Static;

namespace Tests.OpenLoc;

[TestClass]
public class LanguageDataSourceTests
{
    [TestMethod]
    public void Languages_IncludeBaselineCodesWithUniqueValuesAndReadableNames()
    {
        var items = new LanguageDataSourceHandler().GetData().ToList();
        CollectionAssert.IsSubsetOf(new[] { "en", "de", "fr" }, items.Select(x => x.Value).ToArray());
        Assert.AreEqual(items.Count, items.Select(x => x.Value).Distinct().Count());
        Assert.IsTrue(items.All(x => !string.IsNullOrWhiteSpace(x.DisplayName) && x.DisplayName != x.Value));
    }
}
