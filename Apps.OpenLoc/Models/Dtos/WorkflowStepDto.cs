using Newtonsoft.Json;
namespace Apps.OpenLoc.Models.Dtos;
public class WorkflowStepDto
{
    [JsonProperty("assignee_id")] public string? AssigneeId { get; set; }
    [JsonProperty("deadline")] public DateTime? Deadline { get; set; }
    [JsonProperty("id")] public string JobId { get; set; } = string.Empty;
    [JsonProperty("is_last_step")] public bool IsLastStep { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("order")] public int Order { get; set; }
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("type")] public string Type { get; set; } = string.Empty;
}
