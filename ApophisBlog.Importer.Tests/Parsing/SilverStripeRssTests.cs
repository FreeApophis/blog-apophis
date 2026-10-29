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

    [Fact]
    public void Mislabelled_utc_time_on_apophis_ch_is_read_as_swiss_local_time()
    {
        var feed = Feed(Item("Pure Chaos", "http://apophis.ch/blogs/journey-blog/pure-chaos/", "Mon, 14 Jul 2008 15:20:23 +0000"));

        var post = Assert.Single(Parse(feed, "apophis.ch/blogs/journey-blog/rss.xml"));

        Assert.Equal(Language.En, post.Language);
        Assert.Equal(Option.Some(new DateTimeOffset(2008, 7, 14, 15, 20, 23, TimeSpan.FromHours(2))), post.PublishedAt);
    }

    [Fact]
    public void Comment_items_are_not_posts()
    {
        var feed = Feed(
            Item("ut3 Global Player Stats API", "http://apophis.ch/blogs/development-blog/ut3-global-player-stats-api/", "Fri, 19 Feb 2010 16:24:00 +0000"),
            Item("Comment by 'Transporter' on ut3 Global Player Stats API", "http://apophis.ch/blogs/development-blog/ut3-global-player-stats-api/#comment-1", "Sun, 23 Sep 2012 13:32:00 +0000"));

        Assert.Equal(["ut3 Global Player Stats API"], Parse(feed, "apophis.ch/blogs/development-blog/rss.xml").Select(p => p.Title));
    }

    private static string Feed(params string[] items)
        => $"<?xml version=\"1.0\"?><rss version=\"2.0\"><channel>{string.Concat(items)}</channel></rss>";

    private static string Item(string title, string link, string pubDate)
        => $"<item><title>{title}</title><link>{link}</link><description>&lt;p&gt;text&lt;/p&gt;</description><pubDate>{pubDate}</pubDate></item>";

    private static IReadOnlyList<ParsedPost> Parse(string feed, string localFile)
        => SilverStripeRss.Parse(new LoadedFile(new ManifestEntry(localFile, DateTimeOffset.UnixEpoch, "http://" + localFile, "application/rss+xml"), feed))
            .GetOrElse(error => throw new InvalidOperationException(error));
}
