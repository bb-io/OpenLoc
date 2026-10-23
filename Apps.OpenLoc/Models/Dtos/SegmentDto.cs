using Newtonsoft.Json;
namespace Apps.OpenLoc.Models.Dtos;
public class SegmentDto
{
    [JsonProperty("id")] public string SegmentId { get; set; } = string.Empty;
    [JsonProperty("origin")] public string? Origin { get; set; }
    [JsonProperty("position")] public int Position { get; set; }
    [JsonProperty("previous_step")] public StepOutcomeDto? PreviousStep { get; set; }
    [JsonProperty("source_text")] public string SourceText { get; set; } = string.Empty;
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("target_text")] public string? TargetText { get; set; }
    [JsonProperty("tm_match_percent")] public int? TmMatchPercent { get; set; }
    [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; }
}
