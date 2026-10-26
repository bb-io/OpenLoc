using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class SearchProjectJobsResponse
{
    [Display("Jobs", Description = "All matching jobs.")]
    public List<JobResponse> Jobs { get; set; } = [];
}
