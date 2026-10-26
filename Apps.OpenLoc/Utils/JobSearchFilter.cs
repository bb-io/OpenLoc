using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Requests;

namespace Apps.OpenLoc.Utils;

public static class JobSearchFilter
{
    public static void Validate(SearchJobsRequest request)
    {
        InputValidation.Status(request.Status, JobStatusDataSourceHandler.Values);
        if (request.AssigneeId is not null)
            InputValidation.NumericId(request.AssigneeId, "Assigned user ID");
    }

    public static bool Matches(JobDto job, SearchJobsRequest request) =>
        (string.IsNullOrWhiteSpace(request.FileName) || job.FileName.Contains(request.FileName.Trim(), StringComparison.OrdinalIgnoreCase)) &&
        EqualWhenSet(request.Status, job.Status) &&
        EqualWhenSet(request.ProcessingStatus, job.ProcessingStatus) &&
        EqualWhenSet(request.SourceLanguage, job.SourceLanguage) &&
        EqualWhenSet(request.TargetLanguage, job.TargetLanguage) &&
        (request.AssigneeId is null || job.WorkflowStep?.AssigneeId == InputValidation.IdPath(request.AssigneeId, "Assigned user ID"));

    private static bool EqualWhenSet(string? filter, string actual) =>
        string.IsNullOrWhiteSpace(filter) || string.Equals(filter.Trim(), actual, StringComparison.OrdinalIgnoreCase);
}
