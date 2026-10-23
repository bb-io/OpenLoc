using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Events.Polling.Handlers;
using Apps.OpenLoc.Models.Polling;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.OpenLoc.Events.Polling;

[PollingEventList("Projects")]
public class ProjectPollingList(InvocationContext context) : Invocable(context)
{
    [PollingEvent("On project status changed", Description = "Triggers when an existing project's status changes, optionally to a selected status.")]
    public async Task<PollingEventResponse<ProjectStatusMemory, ProjectStatusChangesResponse>> OnProjectStatusChanged(
        PollingEventRequest<ProjectStatusMemory> request,
        [PollingEventParameter] ProjectStatusChangedRequest options)
    {
        InputValidation.Status(options.Status, ProjectStatusDataSourceHandler.Values);
        var projectId = options.ProjectId is null ? null : InputValidation.IdPath(options.ProjectId, "Project ID");
        var projects = projectId is null
            ? await Client.SearchAsync<ProjectDto>("/projects")
            : new List<ProjectDto> { await Client.GetDataAsync<ProjectDto>($"/projects/{projectId}") };
        return ProjectStatusTracker.Evaluate(projects, request.Memory,
            new() { ProjectId = projectId, Status = options.Status });
    }
}
