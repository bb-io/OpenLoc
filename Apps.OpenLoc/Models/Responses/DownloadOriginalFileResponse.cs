using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.OpenLoc.Models.Responses;

public class DownloadOriginalFileResponse
{
    [Display("File", Description = "The original uploaded document, ready for use in another step.")]
    public FileReference File { get; set; } = default!;
}
