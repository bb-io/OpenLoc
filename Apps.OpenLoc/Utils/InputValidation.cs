using System.Globalization;
using Blackbird.Applications.Sdk.Common.Exceptions;
namespace Apps.OpenLoc.Utils;
public static class InputValidation
{
    public static string Required(string? value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PluginMisconfigurationException($"Please fill in '{label}'.");
        return value.Trim();
    }
    public static long NumericId(string? value, string label)
    {
        if (!long.TryParse(Required(value, label), NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
            throw new PluginMisconfigurationException($"Enter a positive numeric value for '{label}'.");
        return id;
    }
    public static string IdPath(string? value, string label) => NumericId(value, label).ToString(CultureInfo.InvariantCulture);
    // Blackbird dates without a time zone are interpreted as UTC.
    public static DateTime UtcDeadline(DateTime value) => value.Kind == DateTimeKind.Unspecified
        ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();

    public static void Status(string? value, IEnumerable<string> choices)
    {
        if (value is not null && !choices.Contains(value))
            throw new PluginMisconfigurationException("Choose a valid status from the dropdown, or leave the status empty.");
    }
}
