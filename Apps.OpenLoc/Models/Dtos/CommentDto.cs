using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class CommentDto
{
    [JsonProperty("id")] public string CommentId { get; set; } = string.Empty;
    [JsonProperty("content")] public string Content { get; set; } = string.Empty;
    [JsonProperty("author_id")] public string? AuthorId { get; set; }
    [JsonProperty("author_name")] public string? AuthorName { get; set; }
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
}
