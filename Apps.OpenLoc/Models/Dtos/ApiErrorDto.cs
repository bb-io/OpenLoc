using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class ApiErrorDto
{
    [JsonProperty("code")] public string? Code { get; set; }
    [JsonProperty("message")] public string? Message { get; set; }
    [JsonProperty("field")] public string? Field { get; set; }
}
