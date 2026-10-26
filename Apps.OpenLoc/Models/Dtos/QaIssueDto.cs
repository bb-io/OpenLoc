using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class QaIssueDto
{
    [JsonProperty("check_type")] public string? CheckType { get; set; }
    [JsonProperty("severity")] public string? Severity { get; set; }
    [JsonProperty("message")] public string? Message { get; set; }
    [JsonProperty("ignored")] public bool Ignored { get; set; }
}
