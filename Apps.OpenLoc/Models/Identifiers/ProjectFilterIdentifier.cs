using Apps.OpenLoc.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Models.Identifiers;

public class ProjectFilterIdentifier
{
    [Display("Project ID", Description = "Optional: select a project to populate related dropdowns."), DataSource(typeof(ProjectDataSourceHandler))]
    public string? ProjectId { get; set; }
}
