using Apps.OpenLoc.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.OpenLoc.Models.Requests;

public class UploadFileRequest : ProjectIdentifier
{
    [Display("File", Description = "A document from a previous step, up to 50 MB.")]
    public FileReference File { get; set; } = default!;

    [Display("Analyze", Description = "Run word-count analysis after the translation jobs are created.")]
    public bool? Analyze { get; set; }

    [Display("XLSX layout", Description = "Optional XLSXLayout JSON text for an XLSX workbook. Leave empty when row 1 contains language codes. Only listed sheets and columns are translated.")]
    public string? XlsxLayout { get; set; }
}
