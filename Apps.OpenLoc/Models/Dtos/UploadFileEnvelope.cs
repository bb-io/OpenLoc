using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class UploadFileEnvelope
{
    [JsonProperty("data")] public FileDto? Data { get; set; }
    [JsonProperty("warnings")] public List<string>? Warnings { get; set; }
}
