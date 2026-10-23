using System.Text.Json;
using Apps.OpenLoc.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Tests.OpenLoc.Base;

public class TestBase
{
    protected AuthenticationCredentialsProvider[] Creds { get; private set; } = [];
    protected InvocationContext InvocationContext { get; private set; } = new();
    protected Dictionary<string, string> TestData { get; } = new();

    [TestInitialize]
    public void LoadConfiguration()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(file))
            Assert.Inconclusive("Copy appsettings.json.example to appsettings.json and configure a real OpenLoc test account.");

        using var document = JsonDocument.Parse(File.ReadAllText(file));
        var root = document.RootElement;
        if (!root.TryGetProperty("ConnectionDefinition", out var connection))
            Assert.Inconclusive("Configure ConnectionDefinition in appsettings.json.");
        Creds = connection.EnumerateObject().Select(x =>
            new AuthenticationCredentialsProvider(x.Name, x.Value.GetString() ?? "")).ToArray();
        if (new[] { CredsNames.Hostname, CredsNames.AccessKey }.Any(name =>
            string.IsNullOrWhiteSpace(Creds.FirstOrDefault(x => x.KeyName == name)?.Value)))
            Assert.Inconclusive("Configure hostname and access_key in appsettings.json.");

        InvocationContext = new() { AuthenticationCredentialsProviders = Creds };
        if (root.TryGetProperty("TestData", out var testData))
            foreach (var property in testData.EnumerateObject())
                TestData[property.Name] = property.Value.ToString();
    }

    protected string RequireTestData(string key)
    {
        if (!TestData.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            Assert.Inconclusive($"Configure TestData.{key} in appsettings.json.");
        return value!;
    }
}
