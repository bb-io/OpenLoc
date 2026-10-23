using Apps.OpenLoc.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.OpenLoc.Models.Requests;

public class SegmentWriteRequest : SegmentIdentifier
{
    [Display("Translation text", Description = "The text to save. Leave empty to keep the current translation.")]
    public string? TranslationText { get; set; }

    [Display("Expected updated at", Description = "Optional updated_at value from the segment, including its time zone and fractional seconds. The write fails if the segment has changed.")]
    public string? ExpectedUpdatedAt { get; set; }
}
