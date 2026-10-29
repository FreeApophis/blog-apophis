namespace ApophisBlog.Importer.Parsing;

using ApophisBlog.Importer.Export;
using ApophisBlog.Importer.Loading;

/// <summary>One archived copy of a post or page as a parser read it; the merge step combines copies into export items.</summary>
public sealed record ParsedPost(
    SourceSystem System,
    ManifestEntry Origin,
    Language Language,
    string Title,
    string Slug,
    Option<string> Blog,
    Body Body,
    Option<DateTimeOffset> PublishedAt)
{
    public IReadOnlyList<string> Tags { get; init; } = [];

    public IReadOnlyList<Comment> Comments { get; init; } = [];
}
