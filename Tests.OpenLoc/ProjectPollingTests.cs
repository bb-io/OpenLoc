using Apps.OpenLoc.Events.Polling;
using Apps.OpenLoc.Models.Polling;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
[TestCategory("Integration")]
public class ProjectPollingTests : TestBase
{
    [TestMethod]
    public async Task FirstPoll_RecordsSelectedProjectWithoutTriggering()
    {
        var projectId = RequireTestData("project_id");
        var response = await new ProjectPollingList(InvocationContext).OnProjectStatusChanged(
            new PollingEventRequest<ProjectStatusMemory>(),
            new() { ProjectId = projectId });
        Assert.IsFalse(response.FlyBird);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        Assert.IsTrue(response.Memory.Statuses.ContainsKey(projectId));
        Assert.AreEqual(0, response.Result.Projects.Count);
    }
}
