using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Models.Requests;

public class UpdateJobRequest : JobIdentifier
{
    [Display("Assigned user ID", Description = "Assign this user to the job's current workflow step."), DataSource(typeof(MemberDataSourceHandler))]
    public string AssigneeId { get; set; } = string.Empty;
}
