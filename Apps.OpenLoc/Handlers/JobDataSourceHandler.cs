using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Identifiers;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.OpenLoc.Handlers;
public class JobDataSourceHandler(InvocationContext context, [ActionParameter] ProjectFilterIdentifier identifier)
    : Invocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier.ProjectId)) return [];
        var id = InputValidation.IdPath(identifier.ProjectId, "Project ID");
        var jobs = await Client.SearchAsync<JobDto>($"/projects/{id}/jobs",
            cancellationToken: cancellationToken, maxResults: 30,
            predicate: x => string.IsNullOrWhiteSpace(context.SearchString) ||
                $"{x.FileName} {x.TargetLanguage} {x.JobId}".Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));
        return jobs.Select(x => new DataSourceItem(x.JobId, $"{x.FileName} ({x.TargetLanguage}) - {x.JobId}"));
    }
}
