using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.OpenLoc.Utils;

public static class XlsxLayoutValidation
{
    public static void ValidateJson(string? layout, string fileName)
    {
        if (layout is null) return;
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new PluginMisconfigurationException("Only supply 'XLSX layout' for an XLSX workbook.");
        try
        {
            var parsed = JObject.Parse(layout);
            if (parsed["sheets"] is not JArray sheets) throw new JsonException();
            foreach (var sheet in sheets)
            {
                if (sheet is not JObject obj || obj["name"]?.Type != JTokenType.String ||
                    string.IsNullOrWhiteSpace(obj["name"]?.Value<string>())) throw new JsonException();
                if (obj["header_row"] is { Type: not JTokenType.Boolean }) throw new JsonException();
                if (obj["columns"] is { } columns && (columns is not JArray || columns.Any(x => x.Type != JTokenType.String)))
                    throw new JsonException();
            }
        }
        catch (JsonException)
        {
            throw new PluginMisconfigurationException("Enter valid XLSXLayout JSON with a 'sheets' array. Each sheet needs a 'name', optional text 'columns', and optional boolean 'header_row'.");
        }
    }

    public static void ValidateHeaders(byte[] workbook, IEnumerable<string> languageCodes)
    {
        var codes = languageCodes.Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var stream = new MemoryStream(workbook, writable: false);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            var sharedEntry = archive.GetEntry("xl/sharedStrings.xml");
            var shared = sharedEntry is null ? [] : LoadXml(sharedEntry).Descendants(ns + "si")
                .Select(x => string.Concat(x.Descendants(ns + "t").Select(t => t.Value))).ToArray();
            foreach (var sheet in archive.Entries.Where(x => x.FullName.StartsWith("xl/worksheets/", StringComparison.Ordinal) && x.FullName.EndsWith(".xml", StringComparison.Ordinal)))
            {
                // Read only row 1; later rows can contain large amounts of document text.
                using var input = sheet.Open();
                using var reader = XmlReader.Create(input, XmlSettings());
                while (reader.Read())
                {
                    if (reader.LocalName != "row" || reader.NodeType != XmlNodeType.Element) continue;
                    if (reader.GetAttribute("r") != "1") break;
                    var row = (XElement)XNode.ReadFrom(reader);
                    foreach (var cell in row.Elements(ns + "c"))
                    {
                        var value = cell.Element(ns + "v")?.Value;
                        var type = (string?)cell.Attribute("t");
                        var text = type == "s" && int.TryParse(value, out var index) && index >= 0 && index < shared.Length
                            ? shared[index] : type == "inlineStr" ? string.Concat(cell.Descendants(ns + "t").Select(x => x.Value)) : value;
                        if (text is not null && codes.Contains(text.Trim()))
                            throw new PluginMisconfigurationException("This workbook has project language codes in row 1. Leave 'XLSX layout' empty so the workbook can be read by its language codes.");
                    }
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is InvalidDataException or XmlException)
        {
            throw new PluginMisconfigurationException("The XLSX workbook could not be read. Select a valid XLSX file before supplying 'XLSX layout'.");
        }
    }

    private static XmlReaderSettings XmlSettings() => new() { DtdProcessing = DtdProcessing.Prohibit };

    private static XDocument LoadXml(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, XmlSettings());
        return XDocument.Load(reader);
    }
}
