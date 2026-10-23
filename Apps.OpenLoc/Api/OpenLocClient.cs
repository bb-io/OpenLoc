using System.Globalization;
using System.Net;
using Apps.OpenLoc.Constants;
using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.OpenLoc.Api;

public class OpenLocClient : BlackBirdRestClient
{
    public OpenLocClient(IEnumerable<AuthenticationCredentialsProvider> credentials)
        : base(new RestClientOptions { BaseUrl = GetOrigin(credentials), Timeout = TimeSpan.FromSeconds(180), FollowRedirects = false })
    {
        var key = credentials.FirstOrDefault(x => x.KeyName == CredsNames.AccessKey)?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Any(char.IsWhiteSpace))
            throw new PluginMisconfigurationException("Enter the access key in the connection without spaces or the Bearer prefix.");
        this.AddDefaultHeader("Authorization", $"Bearer {key}");
        this.AddDefaultHeader("Accept", "application/json");
    }

    private static Uri GetOrigin(IEnumerable<AuthenticationCredentialsProvider> credentials)
    {
        var host = credentials.FirstOrDefault(x => x.KeyName == CredsNames.Hostname)?.Value?.Trim();
        if (!Uri.TryCreate(host, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(uri.Host) ||
            uri.AbsolutePath != "/" || uri.Query.Length > 0 || uri.Fragment.Length > 0 || uri.UserInfo.Length > 0)
            throw new PluginMisconfigurationException("Enter a valid HTTPS API hostname in the connection, without a path, query, or embedded credentials.");
        return uri;
    }

    public async Task<List<MemberDto>> GetMembersAsync(CancellationToken cancellationToken = default)
    {
        // This endpoint returns every member and does not document pagination parameters.
        var page = await SendAsync<PageEnvelope<MemberDto>>(
            new RestRequest("/api/v1/members", Method.Get), cancellationToken);
        return page.Data ?? throw new PluginApplicationException("The API returned no member list. Please try again.");
    }

    public async Task<T> GetDataAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest("/api/v1" + path, body is null ? Method.Get : Method.Post);
        if (body is not null) request.AddJsonBody(body);
        var envelope = await SendAsync<DataEnvelope<T>>(request, cancellationToken);
        return envelope.Data is not null ? envelope.Data : throw new PluginApplicationException("The API returned no result. Please try again.");
    }

    public async Task<List<T>> SearchAsync<T>(string path, Dictionary<string, string?>? filters = null,
        CancellationToken cancellationToken = default, int? maxResults = null, Func<T, bool>? predicate = null)
    {
        var results = new List<T>();
        var seen = new HashSet<long>();
        long? cursor = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new RestRequest("/api/v1" + path).AddQueryParameter("limit", "100");
            foreach (var filter in filters ?? [])
                if (!string.IsNullOrWhiteSpace(filter.Value)) request.AddQueryParameter(filter.Key, filter.Value);
            if (cursor.HasValue) request.AddQueryParameter("cursor", cursor.Value.ToString(CultureInfo.InvariantCulture));
            var page = await SendAsync<PageEnvelope<T>>(request, cancellationToken);
            if (page.Data is null || page.HasMore is null || page.Cursor is null)
                throw new PluginApplicationException("The API returned an incomplete page. Please try again or contact support.");
            var matches = predicate is null ? page.Data : page.Data.Where(predicate);
            results.AddRange(maxResults.HasValue ? matches.Take(maxResults.Value - results.Count) : matches);
            if (maxResults.HasValue && results.Count >= maxResults.Value) return results;
            if (!page.HasMore.Value) return results;
            if (page.Cursor <= 0 || !seen.Add(page.Cursor.Value))
                throw new PluginApplicationException("The API returned an invalid or repeated page cursor. Please contact support.");
            cursor = page.Cursor;
        }
    }

    private async Task<T> SendAsync<T>(RestRequest request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var response = await this.ExecuteAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.TooManyRequests && request.Method == Method.Get && attempt < 3)
            {
                var header = response.Headers?.FirstOrDefault(x => string.Equals(x.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
                var seconds = int.TryParse(header, out var delay) && delay >= 0 ? delay : 3;
                if (seconds <= 60)
                {
                    await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
                    continue;
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (!response.IsSuccessful) throw ConfigureErrorException(response);
            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content ?? "")
                    ?? throw new PluginApplicationException("The API returned an empty response. Please try again.");
            }
            catch (JsonException)
            {
                throw new PluginApplicationException("The API returned an unexpected response. Check the API hostname or contact support.");
            }
        }
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var detail = ExtractError(response.Content);
        var advice = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "The access key was rejected. Replace it in the connection with an active key.",
            HttpStatusCode.Forbidden => "Your access key does not have permission. Ask an administrator to check your role and whether the key is read only.",
            HttpStatusCode.NotFound => "The item was not found. Check the supplied ID and that it belongs to your organization.",
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity => "Check the action inputs and correct the indicated field.",
            HttpStatusCode.Conflict => "The item has changed or its current state prevents this action. Refresh it and try again.",
            HttpStatusCode.TooManyRequests => "The API request limit was reached. Wait before running this action again." + RetryAdvice(response),
            _ => response.StatusCode == 0 ? "Could not reach the API. Check the hostname and try again." : $"The API returned HTTP {(int)response.StatusCode}. Try again or contact support."
        };
        var message = string.IsNullOrWhiteSpace(detail) ? advice : $"{advice} API details: {detail}";
        return (int)response.StatusCode is >= 400 and < 500 && response.StatusCode != HttpStatusCode.TooManyRequests
            ? new PluginMisconfigurationException(message) : new PluginApplicationException(message);
    }

    private static string RetryAdvice(RestResponse response)
    {
        var value = response.Headers?.FirstOrDefault(x =>
            string.Equals(x.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
        return int.TryParse(value, out var seconds) && seconds >= 0
            ? $" Try again after {seconds} seconds." : string.Empty;
    }

    private static string? ExtractError(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        try
        {
            var error = JObject.Parse(content)["error"] as JObject;
            if (error is not null)
            {
                var message = error["message"]?.ToString();
                var field = error["field"]?.ToString();
                var code = error["code"]?.ToString();
                var details = string.Join("; ", new[] { message, field is null ? null : $"Field: {field}", code }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
                if (!string.IsNullOrWhiteSpace(details)) content = details;
            }
        }
        catch (JsonException) { }
        return content.Length > 300 ? content[..300] + "..." : content;
    }
}
