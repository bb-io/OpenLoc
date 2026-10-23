using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Events.Polling.Handlers;
using Apps.OpenLoc.Models.Polling;

namespace Tests.OpenLoc;

[TestClass]
public class ProjectStatusTrackerTests
{
    private static ProjectDto Project(string id, string status) => new() { ProjectId = id, Status = status };
    private static ProjectStatusMemory Memory(string status) => new() { Statuses = new() { ["1"] = status } };

    [TestMethod]
    public void FirstCheck_RecordsStatusesWithoutTriggering()
    {
        var response = ProjectStatusTracker.Evaluate([Project("1", "completed")], null, new());
        Assert.IsFalse(response.FlyBird);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual("completed", response.Memory.Statuses["1"]);
        Assert.AreEqual(0, response.Result.Projects.Count);
    }

    [TestMethod]
    public void RepeatedStatus_DoesNotTriggerAndNewProjectsAreBaselined()
    {
        var response = ProjectStatusTracker.Evaluate(
            [Project("1", "in_progress"), Project("2", "completed")], Memory("in_progress"), new());
        Assert.IsFalse(response.FlyBird);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual("completed", response.Memory.Statuses["2"]);
    }

    [TestMethod]
    public void ChangedStatus_ReturnsPreviousAndCurrentStatus()
    {
        var before = Memory("in_progress");
        var response = ProjectStatusTracker.Evaluate([Project("1", "completed")], before, new());
        Assert.IsTrue(response.FlyBird);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(1, response.Result.Projects.Count);
        Assert.AreEqual("in_progress", response.Result.Projects[0].PreviousStatus);
        Assert.AreEqual("completed", response.Result.Projects[0].Status);
        Assert.AreEqual("in_progress", before.Statuses["1"], "Do not modify previously committed memory.");

        var repeated = ProjectStatusTracker.Evaluate([Project("1", "completed")], response.Memory, new());
        Assert.IsFalse(repeated.FlyBird);
    }

    [TestMethod]
    public void DestinationFilter_TracksNonMatchesAndTriggersOnReentry()
    {
        var options = new ProjectStatusChangedRequest { Status = "completed" };
        var initial = new ProjectStatusMemory { StatusFilter = "completed", Statuses = new() { ["1"] = "completed" } };
        var left = ProjectStatusTracker.Evaluate([Project("1", "in_progress")], initial, options);
        Assert.IsFalse(left.FlyBird);
        Assert.IsNotNull(left.Memory);
        Assert.IsNotNull(left.Result);
        Assert.AreEqual("in_progress", left.Memory.Statuses["1"]);
        var returned = ProjectStatusTracker.Evaluate([Project("1", "completed")], left.Memory, options);
        Assert.IsTrue(returned.FlyBird);
    }

    [TestMethod]
    public void ChangedOptions_ResetBaseline()
    {
        var previous = Memory("in_progress");
        var response = ProjectStatusTracker.Evaluate([Project("1", "completed")], previous,
            new() { ProjectId = "1", Status = "completed" });
        Assert.IsFalse(response.FlyBird);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual("completed", response.Memory.Statuses["1"]);
    }

    [TestMethod]
    public void EmptySnapshot_DropsMissingProjectsWithoutTriggering()
    {
        var response = ProjectStatusTracker.Evaluate([], Memory("completed"), new());
        Assert.IsFalse(response.FlyBird);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(0, response.Result.Projects.Count);
        Assert.AreEqual(0, response.Memory.Statuses.Count);
    }
}
