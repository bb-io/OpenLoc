using Apps.OpenLoc.Api;
using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Requests;
using Apps.OpenLoc.Models.Responses;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.OpenLoc.Actions;

[ActionList("Files")]
public class FileActions : Invocable
{
    private readonly IFileManagementClient fileManagementClient;

    public FileActions(InvocationContext context, IFileManagementClient fileManagementClient) : base(context) =>
        this.fileManagementClient = fileManagementClient;

    internal FileActions(InvocationContext context, IFileManagementClient fileManagementClient, OpenLocClient client) : base(context, client) =>
        this.fileManagementClient = fileManagementClient;

    [Action("Upload file", Description = "Upload a document and create its translation jobs, one per target language.")]
    public async Task<UploadFileResponse> UploadFile([ActionParameter] UploadFileRequest request)
    {
        var projectId = InputValidation.IdPath(request.ProjectId, "Project ID");
        if (request.File is null) throw new PluginMisconfigurationException("Select a file from a previous step in 'File'.");
        var fileName = FileTransfer.SafeFileName(request.File.Name)
            ?? throw new PluginMisconfigurationException("Select a file with a name and extension.");
        if (request.File.Size > FileTransfer.MaxUploadBytes)
            throw new PluginMisconfigurationException("The file exceeds the 50 MB upload limit. Select a smaller file.");
        XlsxLayoutValidation.ValidateJson(request.XlsxLayout, fileName);
        using var stream = await fileManagementClient.DownloadAsync(request.File);
        var bytes = await stream.GetByteData();
        FileTransfer.ValidateUpload(bytes.LongLength);
        if (request.XlsxLayout is not null)
        {
            var project = await Client.GetDataAsync<ProjectDto>($"/projects/{projectId}");
            XlsxLayoutValidation.ValidateHeaders(bytes, project.TargetLanguages.Append(project.SourceLanguage));
        }
        var result = await Client.UploadFileAsync(projectId, bytes, fileName, request.File.ContentType, request.Analyze, request.XlsxLayout);
        return new(result.Data!, result.Warnings);
    }

    [Action("Download translated file", Description = "Download a translated document in its original format or as bilingual XLIFF 1.2.")]
    public async Task<DownloadTranslatedFileResponse> DownloadTranslatedFile([ActionParameter] DownloadTranslatedFileRequest request)
    {
        var jobId = InputValidation.IdPath(request.JobId, "Job ID");
        var response = await Client.DownloadTranslatedFileAsync(jobId, request.ExportFormat);
        var name = FileTransfer.ResponseFileName(response);
        if (name is null)
        {
            var job = await Client.GetDataAsync<JobDto>($"/jobs/{jobId}");
            name = FileTransfer.FallbackFileName(job, request.ExportFormat);
        }
        var contentType = response.ContentType ?? (request.ExportFormat == "xliff" ? "application/x-xliff+xml" : "application/octet-stream");
        using var stream = new MemoryStream(response.RawBytes!, writable: false);
        return new() { File = await fileManagementClient.UploadAsync(stream, contentType, name) };
    }
}
