using Newtonsoft.Json;
namespace Apps.OpenLoc.Models.Dtos;
public class StepOutcomeDto
{
    [JsonProperty("ai_outcome")] public string? AiOutcome { get; set; }
    [JsonProperty("edited")] public bool Edited { get; set; }
    [JsonProperty("end_reason")] public string EndReason { get; set; } = string.Empty;
    [JsonProperty("ended_at")] public DateTime EndedAt { get; set; }
    [JsonProperty("round")] public int Round { get; set; }
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("step_name")] public string StepName { get; set; } = string.Empty;
    [JsonProperty("step_order")] public int StepOrder { get; set; }
    [JsonProperty("step_type")] public string StepType { get; set; } = string.Empty;
}
