namespace ApophisBlog.Importer.Export;

using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ExportPost), "post")]
[JsonDerivedType(typeof(ExportPage), "page")]
public closed record ExportItem(string Id, string Author, IReadOnlyList<Translation> Translations);

public sealed record ExportPost(string Id, string Author, string Blog, IReadOnlyList<Translation> Translations)
    : ExportItem(Id, Author, Translations);

public sealed record ExportPage(string Id, string Author, IReadOnlyList<Translation> Translations)
    : ExportItem(Id, Author, Translations);

public sealed record Translation(
    Language Language,
    string Title,
    string Slug,
    Body Body,
    Option<string> Headline = default,
    Option<DateTimeOffset> PublishedAt = default)
{
    public IReadOnlyList<string> Tags { get; init; } = [];
}

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
