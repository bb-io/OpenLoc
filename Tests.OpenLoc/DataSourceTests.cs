using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
[TestCategory("Integration")]
public class DataSourceTests : TestBase
{
    [TestMethod]
    public async Task ProjectDropdown_RespectsSearch()
    {
        var id = RequireTestData("project_id");
        var project = await new Apps.OpenLoc.Actions.ProjectActions(InvocationContext).GetProject(new() { ProjectId = id });
        var items = (await new ProjectDataSourceHandler(InvocationContext).GetDataAsync(
            new DataSourceContext { SearchString = project.Name }, CancellationToken.None)).ToList();
        Assert.IsTrue(items.Count > 0 && items.Count <= 30);
    }

    [TestMethod]
    public async Task JobDropdown_ReturnsSelectedProjectJobs()
    {
        var items = (await new JobDataSourceHandler(InvocationContext,
            new ProjectFilterIdentifier { ProjectId = RequireTestData("project_id") })
            .GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        Assert.IsTrue(items.Count > 0 && items.Count <= 30, "Choose a project with jobs.");
    }

    [TestMethod]
    public async Task MemberDropdown_ReturnsActiveMembers()
    {
        var items = (await new MemberDataSourceHandler(InvocationContext)
            .GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        Assert.IsTrue(items.Count > 0 && items.Count <= 30);
    }
}
