using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Handlers.Static;

public class LanguageDataSourceHandler : IStaticDataSourceItemHandler
{
    
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("en", "English"),
        new("fr", "French"),
        new("de", "German")
    ];
}
