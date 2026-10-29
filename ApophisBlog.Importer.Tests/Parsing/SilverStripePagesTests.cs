namespace ApophisBlog.Importer.Tests.Parsing;

using ApophisBlog.Importer.Export;
using ApophisBlog.Importer.Loading;
using ApophisBlog.Importer.Parsing;

public sealed class SilverStripePagesTests
{
    [Fact]
    public void Detail_page_becomes_a_full_post()
    {
        var page = Fixture("silverstripe/hello-hongkong.html", "apophis.ch/blogs/journey-blog/hello-hongkong/index.html", "http://apophis.ch/blogs/journey-blog/hello-hongkong/");

        var post = Assert.Single(SilverStripePages.Parse(page).GetOrElse(error => throw new InvalidOperationException(error)));

        Assert.Equal(SourceSystem.SilverStripe, post.System);
        Assert.Equal(Language.En, post.Language);
        Assert.Equal("Hello 香港 (Hongkong)", post.Title);
        Assert.Equal("hello-hongkong", post.Slug);
        Assert.Equal(Option.Some("journey-blog"), post.Blog);
        Assert.Equal(["Hong Kong", "Kowloon", "Tsing Yi", "Visa"], post.Tags);
        Assert.Equal(Option.Some(new DateTimeOffset(2008, 8, 14, 9, 53, 0, TimeSpan.FromHours(2))), post.PublishedAt);
        var body = Assert.IsType<FullBody>(post.Body);
        Assert.StartsWith("<p>No I am already in Hong-Kong", body.Html, StringComparison.Ordinal);
        Assert.EndsWith("Our clocks are 6 hours later than in central europe.</p>", body.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("Comments</p>", body.Html, StringComparison.Ordinal);
    }

    [Fact]
    public void Listing_entries_become_summary_posts_with_their_tags()
    {
        var page = Fixture("silverstripe/blogs-start-20.html", "apophis.ch/blogs/index.html@start=20.html", "http://apophis.ch/blogs/?start=20");

        var posts = SilverStripePages.Parse(page).GetOrElse(error => throw new InvalidOperationException(error));

        Assert.Equal(10, posts.Count);
        var namaste = Assert.Single(posts, p => p.Title == "Namaste Neu Delhi");
        Assert.Equal("namaste-neu-delhi", namaste.Slug);
        Assert.Equal(Option.Some("journey-blog"), namaste.Blog);
        Assert.Equal(["New-Delhi", "India"], namaste.Tags);
        Assert.Equal(Option.Some(new DateTimeOffset(2008, 7, 5, 12, 51, 0, TimeSpan.FromHours(2))), namaste.PublishedAt);
        Assert.Equal(new SummaryBody("<p>It is hot... the weather is sticky... and its raining\nregularly...</p>"), namaste.Body);
    }

    [Fact]
    public void Comments_of_a_detail_page_are_read()
    {
        var page = Fixture("silverstripe/ut3-global-player-stats-api.html", "apophis.ch/blogs/development-blog/ut3-global-player-stats-api/index.html", "http://apophis.ch/blogs/development-blog/ut3-global-player-stats-api/");

        var post = Assert.Single(SilverStripePages.Parse(page).GetOrElse(error => throw new InvalidOperationException(error)));

        var comment = Assert.Single(post.Comments);
        Assert.Equal("silverstripe:apophis.ch:1", comment.Id);
        Assert.Equal("Transporter", comment.Author);
        Assert.Equal("<p>The downloads are not working. I'm still interested in your stats tool.</p>", comment.Html);
        Assert.Equal(Option.Some(new DateTimeOffset(2012, 9, 23, 13, 32, 0, TimeSpan.FromHours(2))), comment.PostedAt);
    }

    private static LoadedFile Fixture(string fixture, string localFile, string originalUrl)
        => new(
            new ManifestEntry(localFile, DateTimeOffset.UnixEpoch, originalUrl, "text/html"),
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture)));
}
