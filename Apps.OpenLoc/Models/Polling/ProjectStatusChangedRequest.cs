using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Models.Polling;

public class ProjectStatusChangedRequest
{
    [Display("Project ID", Description = "Optional: monitor one project. Leave empty to monitor all accessible projects."), DataSource(typeof(ProjectDataSourceHandler))]
    public string? ProjectId { get; set; }

    [Display("Status", Description = "Optional: trigger only when a project changes to this status. Leave empty for any status change."), StaticDataSource(typeof(ProjectStatusDataSourceHandler))]
    public string? Status { get; set; }
}
