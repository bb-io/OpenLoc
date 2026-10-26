using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class StepOutcomeResponse
{
    public StepOutcomeResponse() { }
    public StepOutcomeResponse(StepOutcomeDto dto)
    {
        AiOutcome = dto.AiOutcome;
        Edited = dto.Edited;
        EndReason = dto.EndReason;
        EndedAt = dto.EndedAt;
        Round = dto.Round;
        Status = dto.Status;
        StepName = dto.StepName;
        StepOrder = dto.StepOrder;
        StepType = dto.StepType;
    }
    [Display("AI outcome", Description = "The quality estimation verdict for this step, when available.")]
    public string? AiOutcome { get; set; }
    [Display("Edited", Description = "Whether a person changed the text during this step.")]
    public bool Edited { get; set; }
    [Display("End reason", Description = "Whether the step was completed or sent back to the previous step.")]
    public string EndReason { get; set; } = string.Empty;
    [Display("Ended at", Description = "When the step finished.")]
    public DateTime EndedAt { get; set; }
    [Display("Round", Description = "Which pass through the step, starting at one.")]
    public int Round { get; set; }
    [Display("Status", Description = "The current status of this item.")]
    public string Status { get; set; } = string.Empty;
    [Display("Step name", Description = "The name of the finished workflow step.")]
    public string StepName { get; set; } = string.Empty;
    [Display("Step order", Description = "The position of the step in the workflow, starting at zero.")]
    public int StepOrder { get; set; }
    [Display("Step type", Description = "The kind of work performed in this step.")]
    public string StepType { get; set; } = string.Empty;
}
