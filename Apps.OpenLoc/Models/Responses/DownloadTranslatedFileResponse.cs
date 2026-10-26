using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.OpenLoc.Models.Responses;

public class DownloadTranslatedFileResponse
{
    [Display("File", Description = "The translated document, ready for use in another step.")]
    public FileReference File { get; set; } = default!;
}
