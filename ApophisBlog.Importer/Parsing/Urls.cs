namespace ApophisBlog.Importer.Parsing;

/// <summary>String-based URL helpers; <see cref="Uri"/> is avoided because it may trim trailing dots ("ende...").</summary>
internal static class Urls
{
    /// <summary>Path segments of a URL without scheme and host.</summary>
    public static string[] PathSegments(string url)
        => url.Split("://", 2)[^1].Split('/', StringSplitOptions.RemoveEmptyEntries)[1..];

    /// <summary>Resolves a link against the page URL, dropping a trailing <c>index.html</c> left by the download tool.</summary>
    public static string Resolve(string pageUrl, string href)
    {
        var (scheme, rest) = pageUrl.Split("://", 2) is [var s, var r] ? (s, r) : ("http", pageUrl);
        var host = rest.Split('/')[0];
        var resolved = href switch
        {
            _ when href.Contains("://", StringComparison.Ordinal) => href,
            ['/', ..] => $"{scheme}://{host}{href}",
            _ => $"{scheme}://{string.Join('/', rest.Split('/')[..^1])}/{href}",
        };
        return resolved.EndsWith("/index.html", StringComparison.Ordinal) ? resolved[..^"index.html".Length] : resolved;
    }
}
