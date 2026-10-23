using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Requests;
public class CreateProjectRequest
{
    [Display("Project name", Description = "The name shown on the project page.")]
    public string Name { get; set; } = string.Empty;
    [Display("Source language", Description = "Source language code, for example en.")]
    public string SourceLanguage { get; set; } = string.Empty;
    [Display("Target languages", Description = "At least one target language code, for example de and fr.")]
    public IEnumerable<string> TargetLanguages { get; set; } = [];
    [Display("Description", Description = "Optional notes shown on the project page.")]
    public string? Description { get; set; }
    [Display("Deadline", Description = "Optional due date and time, in UTC.")]
    public DateTime? Deadline { get; set; }
    [Display("Template ID", Description = "Optional numeric ID of a template to apply.")]
    public string? TemplateId { get; set; }
    [Display("Workflow ID", Description = "Optional numeric ID of a workflow; overrides the template workflow.")]
    public string? WorkflowId { get; set; }
}
