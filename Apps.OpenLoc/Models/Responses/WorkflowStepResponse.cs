using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class WorkflowStepResponse
{
    public WorkflowStepResponse() { }
    public WorkflowStepResponse(WorkflowStepDto dto)
    {
        AssigneeId = dto.AssigneeId;
        Deadline = dto.Deadline;
        JobId = dto.JobId;
        IsLastStep = dto.IsLastStep;
        Name = dto.Name;
        Order = dto.Order;
        Status = dto.Status;
        Type = dto.Type;
    }
    [Display("Assigned user ID", Description = "The user assigned to work on this step, when available.")]
    public string? AssigneeId { get; set; }
    [Display("Deadline", Description = "When the work is due.")]
    public DateTime? Deadline { get; set; }
    [Display("Job ID", Description = "The job identifier, representing one file in one target language.")]
    public string JobId { get; set; } = string.Empty;
    [Display("Is last step", Description = "Whether completing this step also completes the job.")]
    public bool IsLastStep { get; set; }
    [Display("Name", Description = "The name shown in the application.")]
    public string Name { get; set; } = string.Empty;
    [Display("Order", Description = "The position in the workflow, starting at zero.")]
    public int Order { get; set; }
    [Display("Status", Description = "The current status of this item.")]
    public string Status { get; set; } = string.Empty;
    [Display("Type", Description = "Whether the work is human, hybrid, or automatic.")]
    public string Type { get; set; } = string.Empty;
}
