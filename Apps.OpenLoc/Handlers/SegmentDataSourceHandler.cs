using Apps.OpenLoc.Api;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Identifiers;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.OpenLoc.Handlers;

public class SegmentDataSourceHandler : Invocable, IAsyncDataSourceItemHandler
{
    private readonly JobIdentifier identifier;

    public SegmentDataSourceHandler(InvocationContext context, [ActionParameter] JobIdentifier identifier) : base(context) => this.identifier = identifier;
    internal SegmentDataSourceHandler(InvocationContext context, JobIdentifier identifier, OpenLocClient client) : base(context, client) => this.identifier = identifier;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier.JobId)) return [];
        var jobId = InputValidation.IdPath(identifier.JobId, "Job ID");
        var segments = await Client.SearchAsync<SegmentDto>($"/jobs/{jobId}/segments",
            new() { ["q"] = context.SearchString }, cancellationToken, maxResults: 30);
        return segments.Select(x => new DataSourceItem(x.SegmentId,
            $"{x.SegmentId}: {(x.SourceText.Length > 100 ? x.SourceText[..100] + "..." : x.SourceText)}"));
    }
}
