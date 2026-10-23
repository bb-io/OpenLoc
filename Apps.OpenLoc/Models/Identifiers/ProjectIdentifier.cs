using Apps.OpenLoc.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.OpenLoc.Models.Identifiers;
public class ProjectIdentifier
{
    [Display("Project ID", Description = "Select a project or enter its numeric ID."), DataSource(typeof(ProjectDataSourceHandler))]
    public string ProjectId { get; set; } = string.Empty;
}
