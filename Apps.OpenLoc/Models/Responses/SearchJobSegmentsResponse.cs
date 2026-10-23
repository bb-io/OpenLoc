using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class SearchJobSegmentsResponse
{
    [Display("Segments", Description = "All matching segments.")]
    public List<SegmentResponse> Segments { get; set; } = [];
}
