using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.OpenLoc.Handlers;
public class ProjectDataSourceHandler(InvocationContext context) : Invocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var projects = await Client.SearchAsync<ProjectDto>("/projects",
            new() { ["q"] = context.SearchString }, cancellationToken, maxResults: 30);
        return projects.Take(30).Select(x => new DataSourceItem(x.ProjectId, x.Name));
    }
}
