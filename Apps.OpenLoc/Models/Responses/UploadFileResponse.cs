using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.OpenLoc.Models.Responses;

public class UploadFileResponse
{
    public UploadFileResponse() { }
    public UploadFileResponse(FileDto file, List<string>? warnings)
    {
        FileId = file.FileId;
        ProjectId = file.ProjectId;
        FileName = file.Name;
        FileFormat = file.Format;
        SegmentCount = file.SegmentCount;
        CreatedAt = file.CreatedAt;
        Warnings = warnings ?? [];
    }

    [Display("File ID")] public string FileId { get; set; } = string.Empty;
    [Display("Project ID")] public string ProjectId { get; set; } = string.Empty;
    [Display("File name")] public string FileName { get; set; } = string.Empty;
    [Display("File format")] public string FileFormat { get; set; } = string.Empty;
    [Display("Segment count")] public int SegmentCount { get; set; }
    [Display("Created date")] public DateTime CreatedAt { get; set; }
    [Display("Warnings", Description = "Work that could not be started after the file was saved. Check these before retrying an upload.")]
    public List<string> Warnings { get; set; } = [];
}
