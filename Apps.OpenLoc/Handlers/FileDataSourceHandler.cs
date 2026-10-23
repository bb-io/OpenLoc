using Apps.OpenLoc.Api;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Identifiers;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.OpenLoc.Handlers;

public class FileDataSourceHandler : Invocable, IAsyncDataSourceItemHandler
{
    private readonly ProjectFilterIdentifier identifier;

    public FileDataSourceHandler(InvocationContext context, [ActionParameter] ProjectFilterIdentifier identifier) : base(context) => this.identifier = identifier;
    internal FileDataSourceHandler(InvocationContext context, ProjectFilterIdentifier identifier, OpenLocClient client) : base(context, client) => this.identifier = identifier;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier.ProjectId)) return [];
        var projectId = InputValidation.IdPath(identifier.ProjectId, "Project ID");
        var files = await Client.SearchAsync<FileDto>($"/projects/{projectId}/files", cancellationToken: cancellationToken,
            maxResults: 30, predicate: x => string.IsNullOrWhiteSpace(context.SearchString) ||
                $"{x.Name} {x.OriginalName} {x.FileId}".Contains(context.SearchString.Trim(), StringComparison.OrdinalIgnoreCase));
        return files.Select(x => new DataSourceItem(x.FileId, $"{x.Name} ({x.FileId})"));
    }
}
