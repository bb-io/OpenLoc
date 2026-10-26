using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.OpenLoc.Handlers;

public class TemplateDataSourceHandler(InvocationContext context) : Invocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var templates = await Client.SearchAsync<TemplateDto>("/templates",
            new() { ["q"] = context.SearchString }, cancellationToken, maxResults: 30);
        return templates.Select(x => new DataSourceItem(x.Id, x.Name));
    }
}
