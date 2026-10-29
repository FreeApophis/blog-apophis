namespace ApophisBlog.Importer.Tests.Parsing;

using ApophisBlog.Importer.Parsing;

public sealed class UrlsTests
{
    [Fact]
    public void Path_segments_keep_trailing_dots()
        => Assert.Equal(["blogs", "reise-blog", "jede-reise-geht-einmal-zu-ende..."], Urls.PathSegments("http://thomasbruderer.ch/blogs/reise-blog/jede-reise-geht-einmal-zu-ende.../"));

    [Theory]
    [InlineData("/blogs/journey-blog/namaste-neu-delhi/", "http://apophis.ch/blogs/journey-blog/namaste-neu-delhi/")]
    [InlineData("journey-blog/hello-hongkong/index.html", "http://apophis.ch/blogs/journey-blog/hello-hongkong/")]
    [InlineData("http://thomasbruderer.ch/blogs/", "http://thomasbruderer.ch/blogs/")]
    public void Links_are_resolved_against_the_page_url(string href, string expected)
        => Assert.Equal(expected, Urls.Resolve("http://apophis.ch/blogs/?start=20", href));
}
