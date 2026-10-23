using Newtonsoft.Json;
namespace Apps.OpenLoc.Models.Dtos;
public class PageEnvelope<T>
{
    [JsonProperty("data")] public List<T>? Data { get; set; }
    [JsonProperty("cursor")] public long? Cursor { get; set; }
    [JsonProperty("has_more")] public bool? HasMore { get; set; }
}
