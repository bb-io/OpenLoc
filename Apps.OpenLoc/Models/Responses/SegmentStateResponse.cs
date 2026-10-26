using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.OpenLoc.Models.Responses;

public class SegmentStateResponse
{
    public SegmentStateResponse() { }
    public SegmentStateResponse(SegmentStateDto dto)
    {
        SegmentId = dto.SegmentId;
        TargetText = dto.TargetText;
        Status = dto.Status;
        UpdatedAt = dto.UpdatedAt;
    }

    [Display("Segment ID")] public string SegmentId { get; set; } = string.Empty;
    [Display("Translation text")] public string? TargetText { get; set; }
    [Display("Status")] public string Status { get; set; } = string.Empty;
    [Display("Updated at", Description = "The exact timestamp to pass as 'Expected updated at' in a later write.")]
    public string UpdatedAt { get; set; } = string.Empty;
}
