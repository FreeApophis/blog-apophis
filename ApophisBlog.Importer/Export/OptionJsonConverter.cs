namespace ApophisBlog.Importer.Export;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

/// <summary>Writes <c>Some(x)</c> as <c>x</c>; <c>None</c> properties are left out (see <see cref="SkipNone"/>).</summary>
public sealed class OptionJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => IsOption(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;

    /// <summary>Contract modifier: a property holding <c>None</c> is not written at all.</summary>
    public static void SkipNone(JsonTypeInfo typeInfo)
    {
        foreach (var property in typeInfo.Properties.Where(p => IsOption(p.PropertyType)))
        {
            var none = Activator.CreateInstance(property.PropertyType);
            property.ShouldSerialize = (_, value) => !Equals(value, none);
        }
    }

    private static bool IsOption(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Option<>);

    private sealed class Converter<T> : JsonConverter<Option<T>>
        where T : notnull
    {
        public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null
                ? Option<T>.None
                : Option.Some(JsonSerializer.Deserialize<T>(ref reader, options)!);

        public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
            => value.Switch(
                none: writer.WriteNullValue,
                some: v => JsonSerializer.Serialize(writer, v, options));
    }
}
