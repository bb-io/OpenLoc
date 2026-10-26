using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class ProjectResponse
{
    public ProjectResponse() { }
    public ProjectResponse(ProjectDto dto)
    {
        CreatedAt = dto.CreatedAt;
        CreatedByUserId = dto.CreatedByUserId;
        Deadline = dto.Deadline;
        Description = dto.Description;
        ProjectId = dto.ProjectId;
        Name = dto.Name;
        SourceLanguage = dto.SourceLanguage;
        Status = dto.Status;
        TargetLanguages = dto.TargetLanguages;
        TemplateId = dto.TemplateId;
        UpdatedAt = dto.UpdatedAt;
    }
    [Display("Created at", Description = "When this item was created.")]
    public DateTime CreatedAt { get; set; }
    [Display("Created by user ID", Description = "The user who created the project.")]
    public string? CreatedByUserId { get; set; }
    [Display("Deadline", Description = "When the work is due.")]
    public DateTime? Deadline { get; set; }
    [Display("Description", Description = "Notes shown on the project page.")]
    public string Description { get; set; } = string.Empty;
    [Display("Project ID", Description = "The project identifier to use in other actions.")]
    public string ProjectId { get; set; } = string.Empty;
    [Display("Name", Description = "The name shown in the application.")]
    public string Name { get; set; } = string.Empty;
    [Display("Source language", Description = "The source language code.")]
    public string SourceLanguage { get; set; } = string.Empty;
    [Display("Status", Description = "The current status of this item.")]
    public string Status { get; set; } = string.Empty;
    [Display("Target languages", Description = "The target language codes.")]
    public List<string> TargetLanguages { get; set; } = [];
    [Display("Template ID", Description = "The template used when creating the project, if any.")]
    public string? TemplateId { get; set; }
    [Display("Updated at", Description = "When this item last changed.")]
    public DateTime UpdatedAt { get; set; }
}
