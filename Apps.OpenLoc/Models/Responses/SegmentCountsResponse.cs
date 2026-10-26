using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class SegmentCountsResponse
{
    public SegmentCountsResponse() { }
    public SegmentCountsResponse(SegmentCountsDto dto)
    {
        Approved = dto.Approved;
        AutoApproved = dto.AutoApproved;
        NeedsReview = dto.NeedsReview;
        PreTranslated = dto.PreTranslated;
        Total = dto.Total;
        Untranslated = dto.Untranslated;
    }
    [Display("Approved", Description = "Segments confirmed by a person.")]
    public int Approved { get; set; }
    [Display("Automatically approved", Description = "Segments approved automatically by quality estimation.")]
    public int AutoApproved { get; set; }
    [Display("Needs review", Description = "Segments waiting for a person to review them.")]
    public int NeedsReview { get; set; }
    [Display("Pre-translated", Description = "Segments filled from translation memory or machine translation but not reviewed.")]
    public int PreTranslated { get; set; }
    [Display("Total", Description = "The total number of segments.")]
    public int Total { get; set; }
    [Display("Untranslated", Description = "Segments without a translation.")]
    public int Untranslated { get; set; }
}
