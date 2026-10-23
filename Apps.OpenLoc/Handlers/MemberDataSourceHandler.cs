using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.OpenLoc.Handlers;

public class MemberDataSourceHandler(InvocationContext context) : Invocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var members = await Client.GetMembersAsync(cancellationToken);
        return members.Where(x => x.Active && (string.IsNullOrWhiteSpace(context.SearchString) ||
                $"{x.Name} {x.Email} {x.UserId}".Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)))
            .Take(30).Select(x => new DataSourceItem(x.UserId, $"{x.Name} ({x.Email})"));
    }
}
