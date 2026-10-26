using Apps.OpenLoc.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.OpenLoc.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups =>
    [
        new()
        {
            Name = "API credentials",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties =
            [
                new(CredsNames.Hostname)
                {
                    DisplayName = "API hostname",
                    Description = "Enter your API hostname including https://, without a path. Do not include /api/v1."
                },
                new(CredsNames.AccessKey)
                {
                    DisplayName = "Access key", Sensitive = true,
                    Description = "From AI settings > Access keys. Paste the key without the Bearer prefix."
                }
            ]
        }
    ];

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values) =>
        values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)).ToArray();
}
