using Blackbird.Applications.Sdk.Common;
using Apps.OpenLoc.Api;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.OpenLoc;

public class Invocable : BaseInvocable
{
    protected OpenLocClient Client { get; }
    public Invocable(InvocationContext context) : base(context) =>
        Client = new(context.AuthenticationCredentialsProviders);
}

