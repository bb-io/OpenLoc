using Newtonsoft.Json;
namespace Apps.OpenLoc.Models.Dtos;
public class JobDto
{
    [JsonProperty("counts")] public SegmentCountsDto Counts { get; set; } = new();
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    [JsonProperty("error_message")] public string? ErrorMessage { get; set; }
    [JsonProperty("file_id")] public string FileId { get; set; } = string.Empty;
    [JsonProperty("file_name")] public string FileName { get; set; } = string.Empty;
    [JsonProperty("id")] public string JobId { get; set; } = string.Empty;
    [JsonProperty("mt_skip_reason")] public string? MtSkipReason { get; set; }
    [JsonProperty("processing_status")] public string ProcessingStatus { get; set; } = string.Empty;
    [JsonProperty("project_id")] public string ProjectId { get; set; } = string.Empty;
    [JsonProperty("segment_count")] public int SegmentCount { get; set; }
    [JsonProperty("source_language")] public string SourceLanguage { get; set; } = string.Empty;
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("target_language")] public string TargetLanguage { get; set; } = string.Empty;
    [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; }
    [JsonProperty("word_count")] public int WordCount { get; set; }
    [JsonProperty("workflow_step")] public WorkflowStepDto? WorkflowStep { get; set; }
}
