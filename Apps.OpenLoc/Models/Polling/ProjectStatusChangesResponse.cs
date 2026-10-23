using Blackbird.Applications.Sdk.Common;

namespace Apps.OpenLoc.Models.Polling;

public class ProjectStatusChangesResponse
{
    [Display("Projects", Description = "Projects whose status changed since the previous check and matches the selected options.")]
    public List<ProjectStatusChangedResponse> Projects { get; set; } = [];
}
