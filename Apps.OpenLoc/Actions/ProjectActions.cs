using Apps.OpenLoc.Handlers.Static;
using System.Globalization;
using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Identifiers;
using Apps.OpenLoc.Models.Requests;
using Apps.OpenLoc.Models.Responses;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.OpenLoc.Actions;
[ActionList("Projects")]
public class ProjectActions(InvocationContext context) : Invocable(context)
{
    [Action("Search projects", Description = "Find all projects matching the selected filters.")]
    public async Task<SearchProjectsResponse> SearchProjects([ActionParameter] SearchProjectsRequest request)
    {
        InputValidation.Status(request.Status, ProjectStatusDataSourceHandler.Values);
        var projects = await Client.SearchAsync<ProjectDto>("/projects", new()
        {
            ["q"] = request.SearchText, ["status"] = request.Status,
            ["source"] = request.SourceLanguage, ["target"] = request.TargetLanguage
        });
        return new() { Projects = projects.Select(x => new ProjectResponse(x)).ToList() };
    }
    [Action("Get project", Description = "Get the details of a selected project.")]
    public async Task<ProjectResponse> GetProject([ActionParameter] ProjectIdentifier request)
    {
        var id = InputValidation.IdPath(request.ProjectId, "Project ID");
        return new(await Client.GetDataAsync<ProjectDto>($"/projects/{id}"));
    }
    [Action("Create project", Description = "Create a translation project with source and target languages.")]
    public async Task<ProjectResponse> CreateProject([ActionParameter] CreateProjectRequest request)
    {
        var name = InputValidation.Required(request.Name, "Project name");
        var source = InputValidation.Required(request.SourceLanguage, "Source language");
        var targets = request.TargetLanguages?.ToArray() ?? [];
        if (targets.Length == 0 || targets.Any(string.IsNullOrWhiteSpace))
            throw new PluginMisconfigurationException("Fill in 'Target languages' with at least one non-empty language code.");
        var body = new Dictionary<string, object>
        {
            ["name"] = name, ["source_language"] = source,
            ["target_languages"] = targets.Select(x => x.Trim()).Distinct().ToArray()
        };
        if (request.Description is not null) body["description"] = request.Description;
        if (request.Deadline.HasValue)
            body["deadline"] = InputValidation.UtcDeadline(request.Deadline.Value).ToString("O", CultureInfo.InvariantCulture);
        if (request.TemplateId is not null) body["template_id"] = InputValidation.NumericId(request.TemplateId, "Template ID");
        if (request.WorkflowId is not null) body["workflow_id"] = InputValidation.NumericId(request.WorkflowId, "Workflow ID");
        return new(await Client.GetDataAsync<ProjectDto>("/projects", body));
    }
}
