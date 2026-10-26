using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.OpenLoc.Utils;

public static class ProjectSearchFilter
{
    public static void Validate(SearchProjectsRequest request)
    {
        InputValidation.Status(request.Status, ProjectStatusDataSourceHandler.Values);
        if (request.DueInHours is <= 0)
            throw new PluginMisconfigurationException("Enter a positive whole number for 'Due in (hours)', or leave it empty.");
        if (request.CreatedAfter.HasValue && request.CreatedBefore.HasValue &&
            InputValidation.UtcDeadline(request.CreatedAfter.Value) > InputValidation.UtcDeadline(request.CreatedBefore.Value))
            throw new PluginMisconfigurationException("Set 'Created before' to a date and time later than 'Created after'.");
    }

    public static bool Matches(ProjectDto project, SearchProjectsRequest request, DateTime? searchTime = null) =>
        (string.IsNullOrWhiteSpace(request.SearchText) || project.Name.Contains(request.SearchText.Trim(), StringComparison.OrdinalIgnoreCase)) &&
        (request.Status is null || project.Status == request.Status) &&
        (!request.CreatedAfter.HasValue || InputValidation.UtcDeadline(project.CreatedAt) > InputValidation.UtcDeadline(request.CreatedAfter.Value)) &&
        (!request.CreatedBefore.HasValue || InputValidation.UtcDeadline(project.CreatedAt) < InputValidation.UtcDeadline(request.CreatedBefore.Value)) &&
        (!request.DueInHours.HasValue || IsDueWithin(project.Deadline, request.DueInHours.Value, searchTime ?? DateTime.UtcNow));

    private static bool IsDueWithin(DateTime? deadline, int hours, DateTime searchTime)
    {
        if (!deadline.HasValue) return false;
        var remainingHours = (InputValidation.UtcDeadline(deadline.Value) - InputValidation.UtcDeadline(searchTime)).TotalHours;
        return remainingHours >= 0 && remainingHours <= hours;
    }
}
