using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.OpenLoc.Models.Responses;

public class CommentResponse
{
    public CommentResponse() { }
    public CommentResponse(CommentDto dto)
    {
        CommentId = dto.CommentId;
        Content = dto.Content;
        AuthorId = dto.AuthorId;
        AuthorName = dto.AuthorName;
        CreatedAt = dto.CreatedAt;
    }

    [Display("Comment ID")] public string CommentId { get; set; } = string.Empty;
    [Display("Comment")] public string Content { get; set; } = string.Empty;
    [Display("Author ID")] public string? AuthorId { get; set; }
    [Display("Author name")] public string? AuthorName { get; set; }
    [Display("Created date")] public DateTime CreatedAt { get; set; }
}
