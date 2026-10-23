using System.Net;
using System.Text;
using Apps.OpenLoc.Api;
using Apps.OpenLoc.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Tests.OpenLoc.Base;

internal static class MockOpenLoc
{
    public static OpenLocClient Client(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) => new(
    [
        new AuthenticationCredentialsProvider(CredsNames.Hostname, "https://example.org"),
        new AuthenticationCredentialsProvider(CredsNames.AccessKey, "test-key")
    ], new ResponseHandler(respond));

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        var response = new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        if (status == HttpStatusCode.TooManyRequests) response.Headers.TryAddWithoutValidation("Retry-After", "0");
        return response;
    }

    private sealed class ResponseHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }
}
