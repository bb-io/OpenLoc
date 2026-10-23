using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.OpenLoc.Handlers.Static;
public class SegmentStatusDataSourceHandler : IStaticDataSourceItemHandler
{
    public static readonly string[] Values = ["needs_review", "auto_approved", "approved"];
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("needs_review", "Needs review"), new("auto_approved", "Automatically approved"),
        new("approved", "Approved")
    ];
}
