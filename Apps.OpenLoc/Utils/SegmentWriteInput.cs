using System.Globalization;
using System.Text.RegularExpressions;
using Apps.OpenLoc.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.OpenLoc.Utils;

public static class SegmentWriteInput
{
    public static Dictionary<string, object> Body(SegmentWriteRequest request)
    {
        var body = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(request.TranslationText)) body["text"] = request.TranslationText;
        if (!string.IsNullOrWhiteSpace(request.ExpectedUpdatedAt))
        {
            var timestamp = request.ExpectedUpdatedAt.Trim();
            if (!Regex.IsMatch(timestamp, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})$") ||
                !DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                throw new PluginMisconfigurationException("Supply the segment's original timestamp in 'Expected updated at', including the time zone, or leave it empty.");
            // Preserve the original precision: converting to DateTime can round the API's timestamp.
            body["expected_updated_at"] = timestamp;
        }
        return body;
    }
}
