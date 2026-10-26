using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
namespace Apps.OpenLoc.Models.Responses;
public class JobResponse
{
    public JobResponse() { }
    public JobResponse(JobDto dto)
    {
        Counts = new(dto.Counts);
        CreatedAt = dto.CreatedAt;
        ErrorMessage = dto.ErrorMessage;
        FileId = dto.FileId;
        FileName = dto.FileName;
        JobId = dto.JobId;
        MtSkipReason = dto.MtSkipReason;
        ProcessingStatus = dto.ProcessingStatus;
        ProjectId = dto.ProjectId;
        SegmentCount = dto.SegmentCount;
        SourceLanguage = dto.SourceLanguage;
        Status = dto.Status;
        TargetLanguage = dto.TargetLanguage;
        UpdatedAt = dto.UpdatedAt;
        WordCount = dto.WordCount;
        WorkflowStep = dto.WorkflowStep is null ? null : new(dto.WorkflowStep);
    }
    [Display("Counts", Description = "The number of segments in each translation state.")]
    public SegmentCountsResponse Counts { get; set; } = new();
    [Display("Created at", Description = "When this item was created.")]
    public DateTime CreatedAt { get; set; }
    [Display("Error message", Description = "Why automatic processing failed, if an error occurred.")]
    public string? ErrorMessage { get; set; }
    [Display("File ID", Description = "The source file identifier.")]
    public string FileId { get; set; } = string.Empty;
    [Display("File name", Description = "The name of the source file.")]
    public string FileName { get; set; } = string.Empty;
    [Display("Job ID", Description = "The job identifier, representing one file in one target language.")]
    public string JobId { get; set; } = string.Empty;
    [Display("Machine translation skip reason", Description = "Why machine translation was skipped: no route or no provider.")]
    public string? MtSkipReason { get; set; }
    [Display("Processing status", Description = "The progress of automatic work, such as pre-translation or quality estimation.")]
    public string ProcessingStatus { get; set; } = string.Empty;
    [Display("Project ID", Description = "The project identifier to use in other actions.")]
    public string ProjectId { get; set; } = string.Empty;
    [Display("Segment count", Description = "The number of segments in the file.")]
    public int SegmentCount { get; set; }
    [Display("Source language", Description = "The source language code.")]
    public string SourceLanguage { get; set; } = string.Empty;
    [Display("Status", Description = "The current status of this item.")]
    public string Status { get; set; } = string.Empty;
    [Display("Target language", Description = "The target language code for this job.")]
    public string TargetLanguage { get; set; } = string.Empty;
    [Display("Updated at", Description = "When this item last changed.")]
    public DateTime UpdatedAt { get; set; }
    [Display("Word count", Description = "The number of source words in the file.")]
    public int WordCount { get; set; }
    [Display("Workflow step", Description = "The workflow step currently active for this job.")]
    public WorkflowStepResponse? WorkflowStep { get; set; }
}
