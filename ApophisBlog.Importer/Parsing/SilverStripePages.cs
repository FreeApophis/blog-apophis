namespace ApophisBlog.Importer.Parsing;

using System.Globalization;
using System.Text.RegularExpressions;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using ApophisBlog.Importer.Export;
using ApophisBlog.Importer.Functional;
using ApophisBlog.Importer.Loading;

/// <summary>Reads SilverStripe blog pages: the full post of a detail page (<c>div.blogEntry</c>) or the summaries of a listing (<c>div.blogSummary</c>).</summary>
public static partial class SilverStripePages
{
    private static readonly HtmlParser Parser = new();

    public static Either<string, IReadOnlyList<ParsedPost>> Parse(LoadedFile file)
    {
        var document = Parser.ParseDocument(file.Text);
        return Option.FromNullable(document.QuerySelector("div.blogEntry"))
            .Match(
                none: () => document.QuerySelectorAll("div.blogSummary:not(.row)")
                    .Select(summary => ListedPost(file.Entry, summary))
                    .Sequence(),
                some: entry => Post(file.Entry, entry, file.Entry.OriginalUrl, html => new FullBody(html))
                    .Select(post => (IReadOnlyList<ParsedPost>)[post with { Comments = Comments(file.Entry, document) }]));
    }

    /// <summary><c>li.comment</c>: "Posted by Name, 23/09/2012 1:32pm (4 years ago)" plus the text in <c>div.comment#comment-N</c>.</summary>
    private static List<Comment> Comments(ManifestEntry origin, IDocument document)
        => document.QuerySelectorAll("#comments-holder li.comment")
            .WhereSelect(item => Comment(origin, item))
            .ToList();

    private static Option<Comment> Comment(ManifestEntry origin, IElement item)
        => from text in Option.FromNullable(item.QuerySelector("div.comment[id^=comment-]"))
           from info in Option.FromNullable(item.QuerySelector("p.info")).Select(p => CommentInfo().Match(p.TextContent)).Where(match => match.Success)
           select new Comment(
               Id: $"silverstripe:{origin.LocalFile.Split('/')[0]}:{text.Id!["comment-".Length..]}",
               Author: info.Groups["author"].Value.Trim(),
               Html: string.Join("\n", text.Children.Where(child => child.LocalName != "i").Select(child => child.OuterHtml)),
               PostedAt: CommentDate(info.Groups["date"].Value));

    private static Option<DateTimeOffset> CommentDate(string date)
        => DateTime.TryParseExact(date, "dd/MM/yyyy h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var clock)
            ? SilverStripeSite.SwissLocal(clock)
            : Option<DateTimeOffset>.None;

    /// <summary>A listing entry links to its post; the link may be absolute or rewritten to a relative local file.</summary>
    private static Either<string, ParsedPost> ListedPost(ManifestEntry origin, IElement summary)
        => Option.FromNullable(summary.QuerySelector("h2.postTitle a")?.GetAttribute("href"))
            .ToEither($"{origin.LocalFile}: blogSummary without post link")
            .SelectMany(href => Post(origin, summary, Urls.Resolve(origin.OriginalUrl, href), html => new SummaryBody(html)));

    private static Either<string, ParsedPost> Post(ManifestEntry origin, IElement entry, string postUrl, Func<string, Body> body)
        => Option.FromNullable(entry.QuerySelector("h2.postTitle"))
            .Select(title => title.TextContent.Trim())
            .ToEither($"{origin.LocalFile}: post without postTitle")
            .Select(title =>
            {
                var path = Urls.PathSegments(postUrl);
                return new ParsedPost(
                    SourceSystem.SilverStripe,
                    origin,
                    SilverStripeSite.LanguageOf(origin),
                    title,
                    Slug: path[^1],
                    Blog: SilverStripeSite.BlogOf(path),
                    body(BodyHtml(entry)),
                    PublishedAt: Option.FromNullable(entry.QuerySelector("a.date")).SelectMany(date => SwissLocalTime(date.TextContent)))
                {
                    Tags = entry.QuerySelectorAll("p.tags a[rel=tag]").Select(tag => tag.TextContent.Trim()).ToList(),
                };
            });

    private static string BodyHtml(IElement entry)
        => string.Join("\n", entry.Children.Where(child => !IsMeta(child)).Select(child => child.OuterHtml));

    private static bool IsMeta(IElement element)
        => element.Matches("a.date, h2.postTitle, p.authorDate, p.tags, p.blogVitals") || CommentCount().IsMatch(element.TextContent);

    /// <summary>Visible date text, e.g. "Aug14th ’08 9:53 am" (the abbr title holds a wrong month, so it is ignored).</summary>
    private static Option<DateTimeOffset> SwissLocalTime(string text)
        => VisibleDate().Match(Regex.Replace(text, @"\s+", " ")) is { Success: true } match
            && DateTime.TryParseExact($"{match.Groups["month"]} {match.Groups["day"]} {match.Groups["year"]} {match.Groups["time"]}", "MMM d yy h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var clock)
                ? SilverStripeSite.SwissLocal(clock)
                : Option<DateTimeOffset>.None;

    [GeneratedRegex(@"^\s*\d+\s+Comments?\s*$")]
    private static partial Regex CommentCount();

    [GeneratedRegex(@"(?<month>[A-Z][a-z]{2})\s*(?<day>\d{1,2})(?:st|nd|rd|th)\s*[’'](?<year>\d{2})\s*(?<time>\d{1,2}:\d{2}\s*[ap]m)")]
    private static partial Regex VisibleDate();

    [GeneratedRegex(@"Posted by (?<author>.+?), (?<date>\d{2}/\d{2}/\d{4} \d{1,2}:\d{2}[ap]m)")]
    private static partial Regex CommentInfo();
}
