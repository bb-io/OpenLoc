using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.OpenLoc.Handlers.Static;
public class ProjectStatusDataSourceHandler : IStaticDataSourceItemHandler
{
    public static readonly string[] Values = ["draft", "in_progress", "completed", "on_hold"];
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("draft", "Draft"), new("in_progress", "In progress"),
        new("completed", "Completed"), new("on_hold", "On hold")
    ];
}
