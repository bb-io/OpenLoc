using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Requests;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Tests.OpenLoc;

[TestClass]
public class JobSearchFilterTests
{
    private static JobDto Job() => new()
    {
        FileName = "Website.docx", Status = "in_progress", ProcessingStatus = "completed",
        SourceLanguage = "en", TargetLanguage = "de", WorkflowStep = new() { AssigneeId = "42" }
    };

    [TestMethod]
    public void NoOptionalFilters_IncludesJob()
    {
        Assert.IsTrue(JobSearchFilter.Matches(Job(), new()));
    }

    [TestMethod]
    public void CombinedFilters_RequireEveryCriterion()
    {
        var filters = new SearchJobsRequest
        {
            FileName = "WEB", Status = "in_progress", ProcessingStatus = "completed",
            SourceLanguage = "EN", TargetLanguage = "de", AssigneeId = "042"
        };
        JobSearchFilter.Validate(filters);
        Assert.IsTrue(JobSearchFilter.Matches(Job(), filters));
        filters.TargetLanguage = "fr";
        Assert.IsFalse(JobSearchFilter.Matches(Job(), filters));
    }

    [TestMethod]
    public void AssigneeFilter_ExcludesUnassignedJobs()
    {
        var job = Job();
        job.WorkflowStep = null;
        Assert.IsFalse(JobSearchFilter.Matches(job, new() { AssigneeId = "42" }));
    }

    [TestMethod]
    public void UnknownStatus_ExplainsHowToCorrectInput()
    {
        Assert.ThrowsException<PluginMisconfigurationException>(() =>
            JobSearchFilter.Validate(new() { Status = "unknown" }));
    }
}
