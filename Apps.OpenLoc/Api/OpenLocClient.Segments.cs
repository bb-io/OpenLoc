using System.Net;
using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.OpenLoc.Api;

public partial class OpenLocClient
{
    public async Task<SegmentStateDto> WriteSegmentAsync(string jobId, string segmentId, Dictionary<string, object> body, bool confirm)
    {
        var path = $"/api/v1/jobs/{jobId}/segments/{segmentId}/{(confirm ? "confirm" : "draft")}";
        var request = new RestRequest(path, confirm ? Method.Post : Method.Put).AddJsonBody(body);
        var result = await SendAsync<DataEnvelope<SegmentStateDto>>(request, CancellationToken.None,
            response => SegmentWriteError(response, confirm));
        return result.Data ?? throw new PluginApplicationException("The API returned no segment state. Retrieve the segment before trying again.");
    }

    private Exception SegmentWriteError(RestResponse response, bool confirm)
    {
        if (response.StatusCode == HttpStatusCode.Conflict)
            return new PluginMisconfigurationException("The segment has changed or its state prevents this write. Retrieve it again, review the current translation, and supply its current 'Expected updated at' before saving. " + ExtractError(response.Content));
        if (confirm && response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            try
            {
                var blocked = JsonConvert.DeserializeObject<QaBlockedDto>(response.Content ?? "");
                var issues = (blocked?.QaIssues ?? []).Select(x =>
                    $"[{x.Severity}] {x.CheckType}: {x.Message} (ignored: {(x.Ignored ? "yes" : "no")})");
                var detail = string.Join("\n", issues);
                var error = blocked?.Error;
                return new PluginMisconfigurationException($"Quality checks blocked confirmation. {error?.Message} " +
                    $"{error?.Code}{(error?.Field is null ? "" : "; field: " + error.Field)}\n{detail}\nFix the reported issues and confirm again. Quality checks were not bypassed.");
            }
            catch (JsonException)
            {
                return new PluginMisconfigurationException("Quality checks blocked confirmation. Review the segment's QA findings before confirming again. " + ExtractError(response.Content));
            }
        }
        return ConfigureErrorException(response);
    }
}
