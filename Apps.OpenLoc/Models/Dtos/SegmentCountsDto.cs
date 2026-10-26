using Newtonsoft.Json;
namespace Apps.OpenLoc.Models.Dtos;
public class SegmentCountsDto
{
    [JsonProperty("approved")] public int Approved { get; set; }
    [JsonProperty("auto_approved")] public int AutoApproved { get; set; }
    [JsonProperty("needs_review")] public int NeedsReview { get; set; }
    [JsonProperty("pre_translated")] public int PreTranslated { get; set; }
    [JsonProperty("total")] public int Total { get; set; }
    [JsonProperty("untranslated")] public int Untranslated { get; set; }
}
