namespace ApophisBlog.Importer.Export;

using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ExportPost), "post")]
public closed record ExportItem(string Id, IReadOnlyList<Translation> Translations);

public sealed record ExportPost(string Id, string Blog, IReadOnlyList<Translation> Translations)
    : ExportItem(Id, Translations);

public sealed record Translation(Language Language, string Title, string Slug, Option<string> Headline = default);

public enum Language
{
    De,
    En,
}
