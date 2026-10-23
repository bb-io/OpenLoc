using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class ProjectLabelDto
{
    [JsonProperty("label_id")] public string LabelId { get; set; } = string.Empty;
    [JsonProperty("label_name")] public string LabelName { get; set; } = string.Empty;
    [JsonProperty("value_id")] public string ValueId { get; set; } = string.Empty;
    [JsonProperty("value")] public string Value { get; set; } = string.Empty;
}
