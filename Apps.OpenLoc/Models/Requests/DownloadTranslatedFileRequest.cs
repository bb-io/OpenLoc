using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.OpenLoc.Models.Requests;

public class DownloadTranslatedFileRequest : JobIdentifier
{
    [Display("Export format", Description = "Original document format by default, or bilingual XLIFF 1.2."), StaticDataSource(typeof(ExportFormatDataSourceHandler))]
    public string? ExportFormat { get; set; }
}
