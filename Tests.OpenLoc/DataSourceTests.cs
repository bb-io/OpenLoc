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

    [TestMethod]
    public async Task CustomerDropdown_ReturnsOnlyCustomerValuesAndHonorsSearch()
    {
        var handler = new CustomerDataSourceHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        Assert.IsTrue(items.Count <= 30);
        if (items.Count == 0) Assert.Inconclusive("Configure at least one customer label value in the test account.");
        var filtered = (await handler.GetDataAsync(new DataSourceContext { SearchString = items[0].DisplayName }, CancellationToken.None)).ToList();
        Assert.IsTrue(filtered.Any(x => x.Value == items[0].Value));
        Assert.IsTrue(filtered.All(x => x.DisplayName.Contains(items[0].DisplayName, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task WorkflowDropdown_ReturnsNamesAndHonorsSearch()
    {
        var handler = new WorkflowDataSourceHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        Assert.IsTrue(items.Count <= 30);
        if (items.Count == 0) Assert.Inconclusive("Configure at least one workflow in the test account.");
        var filtered = (await handler.GetDataAsync(new DataSourceContext { SearchString = items[0].DisplayName }, CancellationToken.None)).ToList();
        Assert.IsTrue(filtered.Any(x => x.Value == items[0].Value));
        Assert.IsTrue(filtered.All(x => x.DisplayName.Contains(items[0].DisplayName, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task TemplateDropdown_ReturnsNamesAndHonorsSearch()
    {
        var handler = new TemplateDataSourceHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        Assert.IsTrue(items.Count <= 30);
        if (items.Count == 0) Assert.Inconclusive("Configure at least one template in the test account.");
        var filtered = (await handler.GetDataAsync(new DataSourceContext { SearchString = items[0].DisplayName }, CancellationToken.None)).ToList();
        Assert.IsTrue(filtered.Any(x => x.Value == items[0].Value));
    }
}
