namespace ApophisBlog.Importer.Tests.Loading;

using ApophisBlog.Importer.Loading;

public sealed class ManifestTests
{
    private const string Header = "local,timestamp,original,mimetype\n";

    [Fact]
    public void Row_with_quoted_fields_is_parsed_into_an_entry()
    {
        var csv = Header + "\"apophis.eu/content/view/1/lang,de/index.html\",20070212193544,\"http://www.apophis.eu:80/content/view/1/lang,de/\",text/html\n";

        var expected = new ManifestEntry(
            LocalFile: "apophis.eu/content/view/1/lang,de/index.html",
            CapturedAt: new DateTimeOffset(2007, 2, 12, 19, 35, 44, TimeSpan.Zero),
            OriginalUrl: "http://www.apophis.eu:80/content/view/1/lang,de/",
            MimeType: "text/html");

        FunctionalAssert.Right(expected, Manifest.Parse(csv).Select(entries => entries.Single()));
    }
}
