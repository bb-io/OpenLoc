using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;

namespace Apps.OpenLoc.Api;

public partial class OpenLocClient
{
    public async Task<RestResponse> DownloadOriginalFileAsync(string fileId)
    {
        var id = InputValidation.IdPath(fileId, "File ID");
        var response = await SendResponseAsync(new RestRequest($"/api/v1/files/{id}/source", Method.Get)
            .AddHeader("Accept", "application/octet-stream"), CancellationToken.None);
        if (response.RawBytes is not { Length: > 0 })
            throw new PluginApplicationException("The original-file download returned an empty file. Check the uploaded document and try again.");
        if (response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true ||
            response.ContentType?.Contains("html", StringComparison.OrdinalIgnoreCase) == true)
            throw new PluginApplicationException("The API returned an unexpected response instead of the original file. Check service availability and try again.");
        return response;
    }

    public async Task<UploadFileEnvelope> UploadFileAsync(string projectId, byte[] bytes, string fileName,
        string? contentType = null, bool? analyze = null, string? xlsxLayout = null)
    {
        var id = InputValidation.IdPath(projectId, "Project ID");
        FileTransfer.ValidateUpload(bytes.LongLength);
        var request = new RestRequest($"/api/v1/projects/{id}/files", Method.Post) { AlwaysMultipartFormData = true };
        request.AddFile("file", bytes, fileName, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        if (analyze.HasValue) request.AddParameter("analyze", analyze.Value ? "true" : "false");
        if (xlsxLayout is not null) request.AddParameter("xlsx_layout", xlsxLayout);
        var result = await SendAsync<UploadFileEnvelope>(request, CancellationToken.None);
        if (result.Data is null || string.IsNullOrWhiteSpace(result.Data.FileId))
            throw new PluginApplicationException("The upload response did not identify a stored file. Check the project's files before uploading again.");
        return result;
    }

    public async Task<RestResponse> DownloadTranslatedFileAsync(string jobId, string? format = null)
    {
        var id = InputValidation.IdPath(jobId, "Job ID");
        FileTransfer.ValidateExportFormat(format);
        var request = new RestRequest($"/api/v1/jobs/{id}/export", Method.Get)
            .AddHeader("Accept", "application/octet-stream, application/x-xliff+xml");
        if (format is not null) request.AddQueryParameter("format", format);
        var response = await SendResponseAsync(request, CancellationToken.None);
        if (response.RawBytes is not { Length: > 0 })
            throw new PluginApplicationException("The export returned an empty file. Check that the job has a document to export and try again.");
        if (response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true ||
            response.ContentType?.Contains("html", StringComparison.OrdinalIgnoreCase) == true)
            throw new PluginApplicationException("The export returned an unexpected response instead of a file. Check service availability and try again.");
        return response;
    }
}
