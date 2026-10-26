using Apps.OpenLoc.Api;
using Apps.OpenLoc.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;

namespace Tests.OpenLoc;

[TestClass]
public class ClientConfigurationTests
{
    [TestMethod]
    public void UserHostname_UsesVersionedProjectsPathAndBearerHeader()
    {
        using var client = new OpenLocClient(
        [
            new AuthenticationCredentialsProvider(CredsNames.Hostname, "https://example.org"),
            new AuthenticationCredentialsProvider(CredsNames.AccessKey, "test-key")
        ]);

        Assert.AreEqual("https://example.org/api/v1/projects",
            client.BuildUri(new RestRequest("/api/v1/projects", Method.Get)).AbsoluteUri);
        Assert.AreEqual("Bearer test-key",
            client.DefaultParameters.Single(x => x.Name == "Authorization").Value);
    }

    [TestMethod]
    public void MissingHostname_RequiresUserInput()
    {
        var error = Assert.ThrowsException<PluginMisconfigurationException>(() => new OpenLocClient(
            [new AuthenticationCredentialsProvider(CredsNames.AccessKey, "test-key")]));

        StringAssert.Contains(error.Message, "HTTPS API hostname");
    }

    [TestMethod]
    public void CustomHostname_PreservesExistingConnectionSupport()
    {
        using var client = new OpenLocClient(
        [
            new AuthenticationCredentialsProvider(CredsNames.Hostname, "https://example.org"),
            new AuthenticationCredentialsProvider(CredsNames.AccessKey, "test-key")
        ]);

        Assert.AreEqual("https://example.org/api/v1/projects",
            client.BuildUri(new RestRequest("/api/v1/projects", Method.Get)).AbsoluteUri);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("https://example.org/api/v1")]
    [DataRow("http://example.org")]
    [DataRow("https://example.org?key=value")]
    public void InvalidHostname_ExplainsRequiredOriginFormat(string hostname)
    {
        var error = Assert.ThrowsException<PluginMisconfigurationException>(() => new OpenLocClient(
        [
            new AuthenticationCredentialsProvider(CredsNames.Hostname, hostname),
            new AuthenticationCredentialsProvider(CredsNames.AccessKey, "test-key")
        ]));

        StringAssert.Contains(error.Message, "HTTPS API hostname");
    }
}
