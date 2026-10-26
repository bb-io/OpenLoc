using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.OpenLoc.Handlers;

public class CustomerDataSourceHandler(InvocationContext context) : Invocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var labels = await Client.GetCollectionAsync<LabelDto>("/labels", cancellationToken);
        var customer = labels.FirstOrDefault(x => x.Key == "customer");
        return (customer?.Values ?? [])
            .Where(x => string.IsNullOrWhiteSpace(context.SearchString) ||
                x.Value.Contains(context.SearchString.Trim(), StringComparison.OrdinalIgnoreCase))
            .Take(30).Select(x => new DataSourceItem(x.Id, x.Value));
    }
}
