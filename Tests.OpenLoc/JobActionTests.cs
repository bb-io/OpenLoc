using Apps.OpenLoc.Actions;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
[TestCategory("Integration")]
public class JobActionTests : TestBase
{
    [TestMethod]
    public async Task SearchProjectJobs_ReturnsConfiguredJob()
    {
        var projectId = RequireTestData("project_id");
        var jobId = RequireTestData("job_id");
        var result = await new JobActions(InvocationContext).SearchProjectJobs(new() { ProjectId = projectId });
        Assert.IsNotNull(result.Jobs);
        Assert.IsTrue(result.Jobs.Any(x => x.JobId == jobId), "Choose a job belonging to the configured project.");
        Assert.IsTrue(result.Jobs.All(x => x.ProjectId == projectId));
    }

    [TestMethod]
    public async Task GetJob_ReturnsConfiguredJob()
    {
        var jobId = RequireTestData("job_id");
        var result = await new JobActions(InvocationContext).GetJob(new() { JobId = jobId });
        Assert.AreEqual(jobId, result.JobId);
        Assert.AreEqual(RequireTestData("project_id"), result.ProjectId);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.FileName));
    }

    [TestMethod]
    public async Task SearchJobs_CombinesOptionalFilters()
    {
        var actions = new JobActions(InvocationContext);
        var job = await actions.GetJob(new() { JobId = RequireTestData("job_id") });
        var request = new Apps.OpenLoc.Models.Requests.SearchJobsRequest
        {
            ProjectId = job.ProjectId, FileName = job.FileName, Status = job.Status,
            ProcessingStatus = job.ProcessingStatus, SourceLanguage = job.SourceLanguage,
            TargetLanguage = job.TargetLanguage, AssigneeId = job.WorkflowStep?.AssigneeId
        };
        var results = await actions.SearchProjectJobs(request);
        Assert.IsTrue(results.Jobs.Any(x => x.JobId == job.JobId));
        Assert.IsTrue(results.Jobs.All(x => x.Status == job.Status && x.TargetLanguage == job.TargetLanguage));
    }

    [TestMethod]
    [TestCategory("UpdatesData")]
    public async Task UpdateJob_ChangesCurrentStepAssignee()
    {
        if (!TestData.TryGetValue("enable_update_job", out var enabled) ||
            !bool.TryParse(enabled, out var allowUpdate) || !allowUpdate)
            Assert.Inconclusive("Set TestData.enable_update_job to true to allow changing a real job assignment.");
        var jobId = RequireTestData("job_id");
        var userId = RequireTestData("assignee_user_id");
        var actions = new JobActions(InvocationContext);
        var result = await actions.UpdateJob(new() { JobId = jobId, AssigneeId = userId });
        Assert.AreEqual(jobId, result.JobId);
        Assert.IsNotNull(result.WorkflowStep);
        Assert.AreEqual(userId, result.WorkflowStep.AssigneeId);
        var saved = await actions.GetJob(new() { JobId = jobId });
        Assert.AreEqual(userId, saved.WorkflowStep?.AssigneeId);
    }
}
