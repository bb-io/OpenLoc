using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Responses;
using Blackbird.Applications.Sdk.Common;

namespace Apps.OpenLoc.Models.Polling;

public class ProjectStatusChangedResponse : ProjectResponse
{
    public ProjectStatusChangedResponse() { }
    public ProjectStatusChangedResponse(ProjectDto project, string previousStatus) : base(project) =>
        PreviousStatus = previousStatus;

    [Display("Previous status", Description = "The project's status at the previous check. Status contains its new status.")]
    public string PreviousStatus { get; set; } = string.Empty;
}
