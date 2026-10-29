namespace ApophisBlog.Importer.Export;

using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ExportPost), "post")]
public closed record ExportItem(string Id, IReadOnlyList<Translation> Translations);

public sealed record ExportPost(string Id, string Blog, IReadOnlyList<Translation> Translations)
    : ExportItem(Id, Translations);

public sealed record Translation(
    Language Language,
    string Title,
    string Slug,
    Body Body,
    Option<string> Headline = default,
    Option<DateTimeOffset> PublishedAt = default);

/// <summary>Whether the archived text is the whole post or only a summary from a listing.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "completeness")]
[JsonDerivedType(typeof(FullBody), "full")]
[JsonDerivedType(typeof(SummaryBody), "summary")]
public closed record Body(string Html);

public sealed record FullBody(string Html) : Body(Html);

public sealed record SummaryBody(string Html) : Body(Html);

public enum Language
{
    De,
    En,
}
