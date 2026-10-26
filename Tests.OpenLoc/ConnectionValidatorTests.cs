using Apps.OpenLoc.Connections;
using Apps.OpenLoc.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
[TestCategory("Integration")]
public class ConnectionValidatorTests : TestBase
{
    [TestMethod]
    public async Task ValidKey_VerifiesProjectAccess()
    {
        var result = await new ConnectionValidator(InvocationContext).ValidateConnection(Creds, CancellationToken.None);
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual("Connection verified.", result.Message, "This test requires an account with project access and a reachable API.");
    }

    [TestMethod]
    public async Task InvalidKey_IsRejected()
    {
        var bad = Creds.Select(x => new AuthenticationCredentialsProvider(x.KeyName,
            x.KeyName == CredsNames.AccessKey ? "invalid-" + Guid.NewGuid().ToString("N") : x.Value));
        var result = await new ConnectionValidator(InvocationContext).ValidateConnection(bad, CancellationToken.None);
        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Message, "access key was rejected");
    }
}
