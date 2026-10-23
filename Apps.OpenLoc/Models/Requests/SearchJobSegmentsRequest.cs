using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
namespace Apps.OpenLoc.Models.Requests;
public class SearchJobSegmentsRequest : JobIdentifier
{
    [Display("Search text", Description = "Find text in the source or translation.")]
    public string? SearchText { get; set; }
    [Display("Status", Description = "Only include segments with this status."), StaticDataSource(typeof(SegmentStatusDataSourceHandler))]
    public string? Status { get; set; }
}
