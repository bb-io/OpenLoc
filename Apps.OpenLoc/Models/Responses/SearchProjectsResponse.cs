using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class SearchProjectsResponse
{
    [Display("Projects", Description = "All matching projects.")]
    public List<ProjectResponse> Projects { get; set; } = [];
}
