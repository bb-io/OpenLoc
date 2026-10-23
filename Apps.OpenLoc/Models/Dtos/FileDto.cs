using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class FileDto
{
    [JsonProperty("id")] public string FileId { get; set; } = string.Empty;
    [JsonProperty("project_id")] public string ProjectId { get; set; } = string.Empty;
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("original_name")] public string? OriginalName { get; set; }
    [JsonProperty("format")] public string Format { get; set; } = string.Empty;
    [JsonProperty("segment_count")] public int SegmentCount { get; set; }
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
}
