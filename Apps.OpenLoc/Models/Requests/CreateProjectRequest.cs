using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.OpenLoc.Models.Requests;
public class CreateProjectRequest
{
    [Display("Project name", Description = "The name shown on the project page.")]
    public string Name { get; set; } = string.Empty;
    [Display("Source language", Description = "The language of the original content."), StaticDataSource(typeof(LanguageDataSourceHandler))]
    public string SourceLanguage { get; set; } = string.Empty;
    [Display("Target languages", Description = "Select one or more languages to translate into."), StaticDataSource(typeof(LanguageDataSourceHandler))]
    public IEnumerable<string> TargetLanguages { get; set; } = [];
    [Display("Description", Description = "Optional notes shown on the project page.")]
    public string? Description { get; set; }
    [Display("Deadline", Description = "Optional due date and time, in UTC.")]
    public DateTime? Deadline { get; set; }
    [Display("Customer label value ID", Description = "Select the customer to assign to the new project."), DataSource(typeof(CustomerDataSourceHandler))]
    public string? CustomerId { get; set; }
    [Display("Template ID", Description = "Optional template to apply, including its settings and labels."), DataSource(typeof(TemplateDataSourceHandler))]
    public string? TemplateId { get; set; }
    [Display("Workflow ID", Description = "Optional workflow to apply; an explicitly selected workflow overrides the template workflow."), DataSource(typeof(WorkflowDataSourceHandler))]
    public string? WorkflowId { get; set; }
}
