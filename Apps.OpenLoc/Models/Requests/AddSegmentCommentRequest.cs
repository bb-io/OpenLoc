using Apps.OpenLoc.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.OpenLoc.Models.Requests;

public class AddSegmentCommentRequest : SegmentIdentifier
{
    [Display("Comment", Description = "A comment of 1 to 5000 characters.")]
    public string Comment { get; set; } = string.Empty;
}
