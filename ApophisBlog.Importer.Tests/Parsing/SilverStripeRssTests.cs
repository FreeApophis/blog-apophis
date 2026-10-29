namespace ApophisBlog.Importer.Tests.Parsing;

using ApophisBlog.Importer.Export;
using ApophisBlog.Importer.Loading;
using ApophisBlog.Importer.Parsing;

public sealed class SilverStripeRssTests
{
    // excerpt of content/thomasbruderer.ch/blogs/reise-blog/rss.xml
    private const string GermanFeed = """
        <?xml version="1.0"?>
        <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:atom="http://www.w3.org/2005/Atom">
          <channel>
            <title>Reise-Blog</title>
            <item>
              <title>Jede Reise geht einmal zu Ende...</title>
              <link>http://thomasbruderer.ch/blogs/reise-blog/jede-reise-geht-einmal-zu-ende.../</link>
              <description>&lt;p&gt;Selbst die sehr langen und interessanten...&lt;/p&gt;</description>
              <pubDate>Sat, 18 Oct 2008 07:27:16 +0200</pubDate>
              <guid>http://thomasbruderer.ch/blogs/reise-blog/jede-reise-geht-einmal-zu-ende.../</guid>
            </item>
          </channel>
        </rss>
        """;

    [Fact]
    public void Post_item_becomes_a_parsed_post()
    {
        var post = Assert.Single(Parse(GermanFeed, "thomasbruderer.ch/blogs/reise-blog/rss.xml"));

        Assert.Equal(SourceSystem.SilverStripe, post.System);
        Assert.Equal(Language.De, post.Language);
        Assert.Equal("Jede Reise geht einmal zu Ende...", post.Title);
        Assert.Equal("jede-reise-geht-einmal-zu-ende...", post.Slug);
        Assert.Equal(Option.Some("reise-blog"), post.Blog);
        Assert.Equal(new FullBody("<p>Selbst die sehr langen und interessanten...</p>"), post.Body);
        Assert.Equal(Option.Some(new DateTimeOffset(2008, 10, 18, 7, 27, 16, TimeSpan.FromHours(2))), post.PublishedAt);
    }

    private static IReadOnlyList<ParsedPost> Parse(string feed, string localFile)
        => SilverStripeRss.Parse(new LoadedFile(new ManifestEntry(localFile, DateTimeOffset.UnixEpoch, "http://" + localFile, "application/rss+xml"), feed))
            .GetOrElse(error => throw new InvalidOperationException(error));
}
