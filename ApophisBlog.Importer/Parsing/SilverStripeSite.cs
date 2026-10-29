namespace ApophisBlog.Importer.Parsing;

using ApophisBlog.Importer.Export;
using ApophisBlog.Importer.Loading;

/// <summary>Conventions shared by SilverStripe feeds and pages on apophis.ch (en) and thomasbruderer.ch (de).</summary>
internal static class SilverStripeSite
{
    private static readonly TimeZoneInfo Zurich = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");

    /// <summary>SilverStripe stored Swiss local time, whatever offset a feed claims.</summary>
    public static DateTimeOffset SwissLocal(DateTime clock)
        => new(clock, Zurich.GetUtcOffset(clock));

    public static Language LanguageOf(ManifestEntry origin)
        => origin.LocalFile.StartsWith("thomasbruderer.ch/", StringComparison.Ordinal) ? Language.De : Language.En;

    /// <summary>Path segments of a URL without scheme and host; string-based because <see cref="Uri"/> may trim trailing dots ("ende...").</summary>
    public static string[] PathSegments(string url)
        => url.Split("://", 2)[^1].Split('/', StringSplitOptions.RemoveEmptyEntries)[1..];

    /// <summary>The blog of <c>/blogs/&lt;blog&gt;/&lt;slug&gt;</c>.</summary>
    public static Option<string> BlogOf(string[] path)
        => path.SkipWhile(segment => segment != "blogs").Skip(1).FirstOrNone();
}
