using Apps.OpenLoc.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
namespace Apps.OpenLoc.Models.Requests;
public class SearchProjectsRequest
{
    [Display("Project name contains", Description = "Find project names containing this text.")]
    public string? SearchText { get; set; }
    [Display("Status", Description = "Only include projects with this status."), StaticDataSource(typeof(ProjectStatusDataSourceHandler))]
    public string? Status { get; set; }
    [Display("Created after", Description = "Only include projects created after this date and time. Dates without a time zone are treated as UTC.")]
    public DateTime? CreatedAfter { get; set; }
    [Display("Created before", Description = "Only include projects created before this date and time. Dates without a time zone are treated as UTC.")]
    public DateTime? CreatedBefore { get; set; }
    [Display("Due in (hours)", Description = "Only include projects due within this many hours from now. Enter a positive whole number. Overdue projects and projects without a deadline are excluded.")]
    public int? DueInHours { get; set; }
}
