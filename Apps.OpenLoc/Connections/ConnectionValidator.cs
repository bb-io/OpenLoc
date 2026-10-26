using Blackbird.Applications.Sdk.Common;
using System.Net;
using Apps.OpenLoc.Api;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
namespace Apps.OpenLoc.Connections;
public class ConnectionValidator(InvocationContext context) : BaseInvocable(context), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        OpenLocClient client;
        try { client = new OpenLocClient(authProviders.ToArray()); }
        catch (PluginMisconfigurationException ex) { return new() { IsValid = false, Message = ex.Message }; }
        using (client)
        {
            var response = await client.ExecuteAsync(new RestRequest("/api/v1/projects").AddQueryParameter("limit", "1"), cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new() { IsValid = false, Message = "The access key was rejected. Enter an active key from AI settings > Access keys." };
            if (response.StatusCode == HttpStatusCode.NotFound)
                return new() { IsValid = false, Message = "The API path was not found. Check the API hostname; /api/v1 is added automatically." };
            return new()
            {
                IsValid = true,
                Message = response.IsSuccessful ? "Connection verified." :
                    "Credentials were not rejected, but the API could not verify access to projects. Check service availability and your account role."
            };
        }
    }
}

