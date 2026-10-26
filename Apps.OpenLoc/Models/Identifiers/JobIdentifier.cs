using Apps.OpenLoc.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.OpenLoc.Models.Identifiers;
public class JobIdentifier : ProjectFilterIdentifier
{
    [Display("Job ID", Description = "Select a job from the project or enter its numeric ID directly."), DataSource(typeof(JobDataSourceHandler))]
    public string JobId { get; set; } = string.Empty;
}
