using Newtonsoft.Json;
namespace Apps.OpenLoc.Models.Dtos;
public class ProjectDto
{
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    [JsonProperty("created_by")] public string? CreatedByUserId { get; set; }
    [JsonProperty("deadline")] public DateTime? Deadline { get; set; }
    [JsonProperty("description")] public string Description { get; set; } = string.Empty;
    [JsonProperty("id")] public string ProjectId { get; set; } = string.Empty;
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("source_language")] public string SourceLanguage { get; set; } = string.Empty;
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("target_languages")] public List<string> TargetLanguages { get; set; } = [];
    [JsonProperty("template_id")] public string? TemplateId { get; set; }
    [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; }
}
