namespace ApophisBlog.Importer.Export;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ExportJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static string Serialize(ExportItem item)
        => JsonSerializer.Serialize(item, Options);
}
