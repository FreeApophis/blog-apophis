namespace ApophisBlog.Importer.Tests.Loading;

using System.Text;

using ApophisBlog.Importer.Loading;

public sealed class HtmlDecodingTests
{
    [Fact]
    public void Valid_utf8_wins_over_a_wrongly_declared_charset()
    {
        var bytes = Encoding.UTF8.GetBytes("""<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1"><p>Handbuch für Fuqid</p>""");

        Assert.Contains("Handbuch für Fuqid", HtmlDecoding.Decode(bytes), StringComparison.Ordinal);
    }
}
