using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Handlers.Static;

public class JobStatusDataSourceHandler : IStaticDataSourceItemHandler
{
    public static readonly string[] Values = ["created", "in_progress", "needs_attention", "completed"];
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("created", "Created"), new("in_progress", "In progress"),
        new("needs_attention", "Needs attention"), new("completed", "Completed")
    ];
}
