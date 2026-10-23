using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Handlers.Static;

public class ExportFormatDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("source", "Original document format"),
        new("xliff", "Bilingual XLIFF 1.2")
    ];
}
