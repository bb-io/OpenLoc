using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Polling;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.OpenLoc.Events.Polling.Handlers;

public static class ProjectStatusTracker
{
    public static PollingEventResponse<ProjectStatusMemory, ProjectStatusChangesResponse> Evaluate(
        IEnumerable<ProjectDto> projects, ProjectStatusMemory? previous, ProjectStatusChangedRequest options)
    {
        var memory = new ProjectStatusMemory { ProjectId = options.ProjectId, StatusFilter = options.Status };
        var changed = new List<ProjectStatusChangedResponse>();
        var sameOptions = previous is not null && previous.ProjectId == options.ProjectId &&
            previous.StatusFilter == options.Status;
        foreach (var project in projects)
        {
            memory.Statuses[project.ProjectId] = project.Status;
            if (sameOptions && previous!.Statuses.TryGetValue(project.ProjectId, out var oldStatus) &&
                oldStatus != project.Status && (options.Status is null || options.Status == project.Status))
                changed.Add(new(project, oldStatus));
        }
        return new()
        {
            FlyBird = changed.Count > 0,
            Result = new() { Projects = changed },
            Memory = memory
        };
    }
}
