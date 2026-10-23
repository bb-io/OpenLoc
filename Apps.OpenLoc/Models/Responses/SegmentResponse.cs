using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class SegmentResponse
{
    public SegmentResponse() { }
    public SegmentResponse(SegmentDto dto)
    {
        SegmentId = dto.SegmentId;
        Origin = dto.Origin;
        Position = dto.Position;
        PreviousStep = dto.PreviousStep is null ? null : new(dto.PreviousStep);
        SourceText = dto.SourceText;
        Status = dto.Status;
        TargetText = dto.TargetText;
        TmMatchPercent = dto.TmMatchPercent;
        UpdatedAt = dto.UpdatedAt;
    }
    [Display("Segment ID", Description = "The segment identifier to use in other actions.")]
    public string SegmentId { get; set; } = string.Empty;
    [Display("Origin", Description = "Where the translation came from: memory, machine translation, automatic post-editing, or file import.")]
    public string? Origin { get; set; }
    [Display("Position", Description = "The position in the document, starting at zero.")]
    public int Position { get; set; }
    [Display("Previous step", Description = "The result of the last finished workflow step, when available.")]
    public StepOutcomeResponse? PreviousStep { get; set; }
    [Display("Source text", Description = "The original text.")]
    public string SourceText { get; set; } = string.Empty;
    [Display("Status", Description = "The current status of this item.")]
    public string Status { get; set; } = string.Empty;
    [Display("Target text", Description = "The translation, or no value if it has not been translated.")]
    public string? TargetText { get; set; }
    [Display("Translation memory match percentage", Description = "The translation memory match score, when available.")]
    public int? TmMatchPercent { get; set; }
    [Display("Updated at", Description = "When this item last changed.")]
    public DateTime UpdatedAt { get; set; }
}
