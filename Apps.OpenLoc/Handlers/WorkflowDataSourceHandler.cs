using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.OpenLoc.Handlers;

public class WorkflowDataSourceHandler(InvocationContext context) : Invocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var workflows = await Client.GetCollectionAsync<WorkflowDto>("/workflows", cancellationToken);
        return workflows.Where(x => string.IsNullOrWhiteSpace(context.SearchString) ||
                x.Name.Contains(context.SearchString.Trim(), StringComparison.OrdinalIgnoreCase))
            .Take(30).Select(x => new DataSourceItem(x.Id, x.Name));
    }
}
