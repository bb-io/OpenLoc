using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class WorkflowDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}
