using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Identifiers;
using Apps.OpenLoc.Models.Requests;
using Apps.OpenLoc.Models.Responses;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.OpenLoc.Actions;
[ActionList("Jobs")]
public class JobActions(InvocationContext context) : Invocable(context)
{
    [Action("Search jobs", Description = "Find jobs in a project matching optional filters.")]
    public async Task<SearchProjectJobsResponse> SearchProjectJobs([ActionParameter] SearchJobsRequest request)
    {
        var id = InputValidation.IdPath(request.ProjectId, "Project ID");
        JobSearchFilter.Validate(request);
        var jobs = await Client.SearchAsync<JobDto>($"/projects/{id}/jobs",
            predicate: job => JobSearchFilter.Matches(job, request));
        return new() { Jobs = jobs.Select(x => new JobResponse(x)).ToList() };
    }

    [Action("Get job", Description = "Get a job's details, progress, and current workflow step.")]
    public async Task<JobResponse> GetJob([ActionParameter] JobIdentifier request)
    {
        var id = InputValidation.IdPath(request.JobId, "Job ID");
        return new(await Client.GetDataAsync<JobDto>($"/jobs/{id}"));
    }

    [Action("Update job", Description = "Change the user assigned to a job's current workflow step.")]
    public async Task<JobResponse> UpdateJob([ActionParameter] UpdateJobRequest request)
    {
        var id = InputValidation.IdPath(request.JobId, "Job ID");
        var userId = InputValidation.NumericId(request.AssigneeId, "Assigned user ID");
        return new(await Client.GetDataAsync<JobDto>($"/jobs/{id}/assign", new { user_id = userId }));
    }
}
