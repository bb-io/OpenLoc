using Apps.OpenLoc.Actions;
using Apps.OpenLoc.Models.Identifiers;
using Apps.OpenLoc.Models.Requests;
using Tests.OpenLoc.Base;

namespace Tests.OpenLoc;

[TestClass]
[TestCategory("Integration")]
public class ProjectActionTests : TestBase
{
    [TestMethod]
    public async Task SearchProjects_ReturnsConfiguredProject()
    {
        var id = RequireTestData("project_id");
        var result = await new ProjectActions(InvocationContext).SearchProjects(new());
        Assert.IsNotNull(result.Projects);
        Assert.IsTrue(result.Projects.Any(x => x.ProjectId == id), "The configured project should be present in the complete results.");
    }

    [TestMethod]
    public async Task GetProject_ReturnsConfiguredProject()
    {
        var id = RequireTestData("project_id");
        var result = await new ProjectActions(InvocationContext).GetProject(new ProjectIdentifier { ProjectId = id });
        Assert.AreEqual(id, result.ProjectId);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Name));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.SourceLanguage));
    }

    [TestMethod]
    [TestCategory("CreatesData")]
    public async Task CreateProject_PersistsNameAndLanguages()
    {
        if (!TestData.TryGetValue("enable_create_project", out var enabled) ||
            !bool.TryParse(enabled, out var allowCreate) || !allowCreate)
            Assert.Inconclusive("Set TestData.enable_create_project to true to allow creating a real project.");

        var name = $"Blackbird integration test {Guid.NewGuid():N}";
        var source = RequireTestData("source_language");
        var target = RequireTestData("target_language");
        var actions = new ProjectActions(InvocationContext);
        var created = await actions.CreateProject(new CreateProjectRequest
        {
            Name = name, SourceLanguage = source, TargetLanguages = [target],
            Description = "Disposable project created by the Blackbird OpenLoc integration tests."
        });
        Assert.IsFalse(string.IsNullOrWhiteSpace(created.ProjectId));
        Console.WriteLine($"Created project {created.ProjectId}: {name}. Remove it manually when finished.");
        var saved = await actions.GetProject(new() { ProjectId = created.ProjectId });
        Assert.AreEqual(name, saved.Name);
        Assert.AreEqual(source, saved.SourceLanguage);
        CollectionAssert.Contains(saved.TargetLanguages, target);
    }
}
