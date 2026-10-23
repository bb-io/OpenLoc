using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Requests;
using Apps.OpenLoc.Models.Responses;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.OpenLoc.Actions;
[ActionList("Segments")]
public class SegmentActions(InvocationContext context) : Invocable(context)
{
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
