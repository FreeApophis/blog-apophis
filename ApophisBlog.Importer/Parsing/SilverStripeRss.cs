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
    private static readonly TimeZoneInfo Zurich = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");

    public static Either<string, IReadOnlyList<ParsedPost>> Parse(LoadedFile file)
        => LoadXml(file).SelectMany(feed => feed
            .Descendants("item")
            .Select(item => ToPost(file.Entry, item))
            .Sequence());

    private static Either<string, ParsedPost> ToPost(ManifestEntry origin, XElement item)
        => from title in Required(origin, item, "title")
           from link in Required(origin, item, "link")
           from html in Required(origin, item, "description")
           let path = PathSegments(link)
           select new ParsedPost(
               SourceSystem.SilverStripe,
               origin,
               LanguageOf(origin),
               title,
               Slug: path[^1],
               Blog: path.SkipWhile(segment => segment != "blogs").Skip(1).FirstOrNone(),
               new FullBody(html),
               PublishedAt: Optional(item, "pubDate").SelectMany(SwissLocalTime));

    private static Either<string, string> Required(ManifestEntry origin, XElement item, string name)
        => Optional(item, name).ToEither($"{origin.LocalFile}: <item> without <{name}>");

    private static Option<string> Optional(XElement item, string name)
        => Option.FromNullable(item.Element(name)).Select(element => element.Value.Trim());

    /// <summary>Path segments of a URL without scheme and host; string-based because <see cref="Uri"/> may trim trailing dots ("ende...").</summary>
    private static string[] PathSegments(string url)
        => url.Split("://", 2)[^1].Split('/', StringSplitOptions.RemoveEmptyEntries)[1..];

    private static Language LanguageOf(ManifestEntry origin)
        => origin.LocalFile.StartsWith("thomasbruderer.ch/", StringComparison.Ordinal) ? Language.De : Language.En;

    /// <summary>SilverStripe stored Swiss local time; the apophis.ch feed labels it +0000, so the label is ignored.</summary>
    private static Option<DateTimeOffset> SwissLocalTime(string rfc822)
        => DateTime.TryParseExact(rfc822[..Math.Min(rfc822.Length, 25)], "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var clock)
            ? new DateTimeOffset(clock, Zurich.GetUtcOffset(clock))
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
