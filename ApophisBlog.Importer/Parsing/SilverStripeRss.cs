namespace ApophisBlog.Importer.Parsing;

using System.Globalization;
using System.Xml;
using System.Xml.Linq;

using ApophisBlog.Importer.Export;
using ApophisBlog.Importer.Functional;
using ApophisBlog.Importer.Loading;

/// <summary>Reads SilverStripe blog feeds: full post bodies with their publication time.</summary>
public static class SilverStripeRss
{
    public static Either<string, IReadOnlyList<ParsedPost>> Parse(LoadedFile file)
        => LoadXml(file).SelectMany(feed => feed
            .Descendants("item")
            .Where(item => !IsComment(item))
            .Select(item => ToPost(file.Entry, item))
            .Sequence());

    private static Either<string, ParsedPost> ToPost(ManifestEntry origin, XElement item)
        => from title in Required(origin, item, "title")
           from link in Required(origin, item, "link")
           from html in Required(origin, item, "description")
           let path = SilverStripeSite.PathSegments(link)
           select new ParsedPost(
               SourceSystem.SilverStripe,
               origin,
               SilverStripeSite.LanguageOf(origin),
               title,
               Slug: path[^1],
               Blog: SilverStripeSite.BlogOf(path),
               new FullBody(html),
               PublishedAt: Optional(item, "pubDate").SelectMany(SwissLocalTime));

    /// <summary>Comment feeds and blog feeds mix in comments as items linking to <c>…/#comment-N</c>.</summary>
    private static bool IsComment(XElement item)
        => Optional(item, "link").Match(none: false, some: link => link.Contains("#comment-", StringComparison.Ordinal));

    private static Either<string, string> Required(ManifestEntry origin, XElement item, string name)
        => Optional(item, name).ToEither($"{origin.LocalFile}: <item> without <{name}>");

    private static Option<string> Optional(XElement item, string name)
        => Option.FromNullable(item.Element(name)).Select(element => element.Value.Trim());

    /// <summary>The offset label is ignored: the apophis.ch feed claims +0000 for Swiss local time.</summary>
    private static Option<DateTimeOffset> SwissLocalTime(string rfc822)
        => DateTime.TryParseExact(rfc822[..Math.Min(rfc822.Length, 25)], "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var clock)
            ? SilverStripeSite.SwissLocal(clock)
            : Option<DateTimeOffset>.None;

    private static Either<string, XDocument> LoadXml(LoadedFile file)
    {
        try
        {
            return XDocument.Parse(file.Text);
        }
        catch (XmlException exception)
        {
            return Either<string, XDocument>.Left($"{file.Entry.LocalFile}: {exception.Message}");
        }
    }
}
