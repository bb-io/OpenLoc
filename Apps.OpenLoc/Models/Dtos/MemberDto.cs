using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class MemberDto
{
    [JsonProperty("id")] public string UserId { get; set; } = string.Empty;
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("email")] public string Email { get; set; } = string.Empty;
    [JsonProperty("active")] public bool Active { get; set; }
}
