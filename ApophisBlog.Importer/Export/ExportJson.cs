namespace ApophisBlog.Importer.Export;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

public static class ExportJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new OptionJsonConverter() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { OptionJsonConverter.SkipNone } },
    };

    public static string Serialize(ExportItem item)
        => JsonSerializer.Serialize(item, Options);

    /// <summary>Reads an exported item; malformed JSON is returned as an error message, not thrown.</summary>
    public static Either<string, ExportItem> Deserialize(string json)
    {
        try
        {
            return Option.FromNullable(JsonSerializer.Deserialize<ExportItem>(json, Options))
                .ToEither("The JSON document is null.");
        }
        catch (JsonException exception)
        {
            return Either<string, ExportItem>.Left(exception.Message);
        }
    }
}
