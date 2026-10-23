using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class SegmentStateDto
{
    [JsonProperty("id")] public string SegmentId { get; set; } = string.Empty;
    [JsonProperty("target_text")] public string? TargetText { get; set; }
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("updated_at")] public string UpdatedAt { get; set; } = string.Empty;
}
