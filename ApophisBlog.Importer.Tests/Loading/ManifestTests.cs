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

    [Fact]
    public void Invalid_timestamp_is_an_error_naming_the_line()
    {
        var csv = Header + "a.html,20070212193544,http://a/,text/html\nb.html,not-a-date,http://b/,text/html\n";

        FunctionalAssert.Left("line 3: invalid timestamp 'not-a-date'", Manifest.Parse(csv));
    }

    [Fact]
    public void File_listed_twice_keeps_its_last_row()
    {
        var csv = Header + "apophis.eu/node/106/index.html,20101206000000,http://apophis.eu/node/106,text/html\n"
                         + "apophis.eu/node/106/index.html,20110619000000,http://apophis.eu/node/106/,text/html\n";

        FunctionalAssert.Right(["http://apophis.eu/node/106/"], Manifest.Parse(csv).Select(entries => entries.Select(e => e.OriginalUrl).ToList()));
    }
}
