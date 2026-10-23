using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Api;
using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Requests;
using Apps.OpenLoc.Models.Responses;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.OpenLoc.Actions;
[ActionList("Segments")]
public class SegmentActions : Invocable
{
    public SegmentActions(InvocationContext context) : base(context) { }
    internal SegmentActions(InvocationContext context, OpenLocClient client) : base(context, client) { }

    [Action("Save draft translation", Description = "Save translation text without approving it; the segment returns to needs review.")]
    public async Task<SegmentStateResponse> SaveDraftTranslation([ActionParameter] SegmentWriteRequest request)
    {
        var jobId = InputValidation.IdPath(request.JobId, "Job ID");
        var segmentId = InputValidation.IdPath(request.SegmentId, "Segment ID");
        return new(await Client.WriteSegmentAsync(jobId, segmentId, SegmentWriteInput.Body(request), confirm: false));
    }

    [Action("Confirm segment translation", Description = "Approve a translation and save it to the project's writable translation memories.")]
    public async Task<SegmentStateResponse> ConfirmTranslation([ActionParameter] SegmentWriteRequest request)
    {
        var jobId = InputValidation.IdPath(request.JobId, "Job ID");
        var segmentId = InputValidation.IdPath(request.SegmentId, "Segment ID");
        return new(await Client.WriteSegmentAsync(jobId, segmentId, SegmentWriteInput.Body(request), confirm: true));
    }

    [Action("Add segment comment", Description = "Add a comment to a translation segment.")]
    public async Task<CommentResponse> AddSegmentComment([ActionParameter] AddSegmentCommentRequest request)
    {
        var jobId = InputValidation.IdPath(request.JobId, "Job ID");
        var segmentId = InputValidation.IdPath(request.SegmentId, "Segment ID");
        if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Length > 5000)
            throw new PluginMisconfigurationException("Enter a non-empty 'Comment' containing 1 to 5000 characters.");
        var dto = await Client.GetDataAsync<CommentDto>($"/jobs/{jobId}/segments/{segmentId}/comments", new { content = request.Comment });
        return new(dto);
    }

    [Action("Search job segments", Description = "Get source text and translations for all matching segments in a job.")]
    public async Task<SearchJobSegmentsResponse> SearchJobSegments([ActionParameter] SearchJobSegmentsRequest request)
    {
        var id = InputValidation.IdPath(request.JobId, "Job ID");
        InputValidation.Status(request.Status, SegmentStatusDataSourceHandler.Values);
        var segments = await Client.SearchAsync<SegmentDto>($"/jobs/{id}/segments",
            new() { ["q"] = request.SearchText, ["status"] = request.Status });
        return new() { Segments = segments.Select(x => new SegmentResponse(x)).ToList() };
    }
}
