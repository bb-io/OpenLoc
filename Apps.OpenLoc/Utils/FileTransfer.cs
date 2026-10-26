using System.Net.Http.Headers;
using Apps.OpenLoc.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;

namespace Apps.OpenLoc.Utils;

public static class FileTransfer
{
    public const long MaxUploadBytes = 50L * 1024 * 1024;

    public static void ValidateUpload(long size)
    {
        if (size <= 0) throw new PluginMisconfigurationException("Select a non-empty file to upload.");
        if (size > MaxUploadBytes) throw new PluginMisconfigurationException("The file exceeds the 50 MB upload limit. Select a smaller file.");
    }

    public static void ValidateExportFormat(string? format)
    {
        if (format is not null && format is not ("source" or "xliff"))
            throw new PluginMisconfigurationException("Select an export format from the dropdown, or leave it empty for the original document format.");
    }

    public static string? ResponseFileName(RestResponse response)
    {
        var header = (response.ContentHeaders ?? []).Concat(response.Headers ?? []).FirstOrDefault(x =>
            string.Equals(x.Name, "Content-Disposition", StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
        if (!ContentDispositionHeaderValue.TryParse(header, out var disposition)) return null;
        return SafeFileName(disposition.FileNameStar) ?? SafeFileName(disposition.FileName);
    }

    public static string FallbackFileName(JobDto job, string? format)
    {
        var original = SafeFileName(job.FileName) ?? $"job-{job.JobId}";
        var name = format == "xliff" ? Path.ChangeExtension(original, ".xlf") : original;
        return name;
    }

    public static string? SafeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim('"').Replace('\\', '/').Split('/').Last();
        if (string.IsNullOrWhiteSpace(name) || name is "." or "..") return null;
        return string.Concat(name.Select(c => char.IsControl(c) || "<>:\"|?*".Contains(c) ? '_' : c));
    }
}
