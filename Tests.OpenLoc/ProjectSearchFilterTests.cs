using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Requests;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Tests.OpenLoc;

[TestClass]
public class ProjectSearchFilterTests
{
    private static readonly DateTime CreatedAt = new(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
    private static ProjectDto Project => new() { Name = "Website translation", Status = "in_progress", CreatedAt = CreatedAt };

    [TestMethod]
    public void OptionalFilters_CombineNameStatusAndCreationWindow()
    {
        var request = new SearchProjectsRequest
        {
            SearchText = "WEBSITE", Status = "in_progress",
            CreatedAfter = CreatedAt.AddDays(-1), CreatedBefore = CreatedAt.AddDays(1)
        };
        Assert.IsTrue(ProjectSearchFilter.Matches(Project, request));
        request.SearchText = "mobile";
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, request));
        request.SearchText = null;
        request.Status = "completed";
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, request));
        Assert.IsTrue(ProjectSearchFilter.Matches(Project, new()));
    }

    [TestMethod]
    public void CreationWindow_ExcludesBoundariesAndOutsideDates()
    {
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, new() { CreatedAfter = CreatedAt }));
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, new() { CreatedBefore = CreatedAt }));
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, new() { CreatedAfter = CreatedAt.AddSeconds(1) }));
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, new() { CreatedBefore = CreatedAt.AddSeconds(-1) }));
        Assert.IsTrue(ProjectSearchFilter.Matches(Project, new() { CreatedAfter = CreatedAt.AddSeconds(-1) }));
        Assert.IsTrue(ProjectSearchFilter.Matches(Project, new() { CreatedBefore = CreatedAt.AddSeconds(1) }));
    }

    [TestMethod]
    public void CreationWindow_NormalizesLocalAndUnspecifiedTimes()
    {
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, new() { CreatedAfter = CreatedAt.ToLocalTime() }));
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, new() { CreatedBefore = DateTime.SpecifyKind(CreatedAt, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void ReversedCreationWindow_ExplainsHowToCorrectInputs()
    {
        var error = Assert.ThrowsException<PluginMisconfigurationException>(() => ProjectSearchFilter.Validate(
            new() { CreatedAfter = CreatedAt, CreatedBefore = CreatedAt.AddDays(-1) }));
        StringAssert.Contains(error.Message, "Created before");
        StringAssert.Contains(error.Message, "Created after");
    }

    [DataTestMethod]
    [DataRow(-1.0, false)]
    [DataRow(0.0, true)]
    [DataRow(12.0, true)]
    [DataRow(24.0, true)]
    [DataRow(24.01, false)]
    public void DueWindow_IncludesUpcomingDeadlinesAndBoundaries(double hoursUntilDue, bool expected)
    {
        var project = Project;
        project.Deadline = CreatedAt.AddHours(hoursUntilDue);
        var request = new SearchProjectsRequest
        {
            SearchText = "WEBSITE", Status = "in_progress",
            CreatedAfter = CreatedAt.AddDays(-1), CreatedBefore = CreatedAt.AddDays(1), DueInHours = 24
        };
        Assert.AreEqual(expected, ProjectSearchFilter.Matches(project, request, CreatedAt));
        project.Deadline = project.Deadline.Value.ToLocalTime();
        Assert.AreEqual(expected, ProjectSearchFilter.Matches(project, request, CreatedAt));
    }

    [TestMethod]
    public void DueWindow_ExcludesMissingDeadlinesOnlyWhenFilterIsSet()
    {
        Assert.IsFalse(ProjectSearchFilter.Matches(Project, new() { DueInHours = 24 }, CreatedAt));
        Assert.IsTrue(ProjectSearchFilter.Matches(Project, new(), CreatedAt));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void InvalidDueWindow_ExplainsHowToCorrectInput(int hours)
    {
        var error = Assert.ThrowsException<PluginMisconfigurationException>(() =>
            ProjectSearchFilter.Validate(new() { DueInHours = hours }));
        StringAssert.Contains(error.Message, "Due in (hours)");
    }
}
