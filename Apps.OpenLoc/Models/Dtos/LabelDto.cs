using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class LabelDto
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    [JsonProperty("key")] public string? Key { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("values")] public List<LabelValueDto> Values { get; set; } = [];
}
