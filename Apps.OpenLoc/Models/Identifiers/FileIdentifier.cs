using Apps.OpenLoc.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Models.Identifiers;

public class FileIdentifier : ProjectFilterIdentifier
{
    [Display("File ID", Description = "Select a file from the chosen project, or enter its file ID directly."), DataSource(typeof(FileDataSourceHandler))]
    public string FileId { get; set; } = string.Empty;
}
