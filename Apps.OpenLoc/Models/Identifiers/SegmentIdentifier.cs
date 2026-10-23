using Apps.OpenLoc.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Models.Identifiers;

public class SegmentIdentifier : JobIdentifier
{
    [Display("Segment ID", Description = "Select a segment from the chosen job, or enter its segment ID."), DataSource(typeof(SegmentDataSourceHandler))]
    public string SegmentId { get; set; } = string.Empty;
}
