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

    private static LoadedFile Fixture(string fixture, string localFile, string originalUrl)
        => new(
            new ManifestEntry(localFile, DateTimeOffset.UnixEpoch, originalUrl, "text/html"),
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture)));
}
