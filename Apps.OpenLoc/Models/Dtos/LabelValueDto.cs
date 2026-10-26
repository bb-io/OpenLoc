using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class LabelValueDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    [JsonProperty("value")] public string Value { get; set; } = string.Empty;
}
