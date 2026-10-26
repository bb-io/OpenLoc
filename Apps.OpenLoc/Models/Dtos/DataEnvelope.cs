using Newtonsoft.Json;
namespace Apps.OpenLoc.Models.Dtos;
public class DataEnvelope<T>
{
    [JsonProperty("data")] public T? Data { get; set; }
}
