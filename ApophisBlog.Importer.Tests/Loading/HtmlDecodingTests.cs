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

    [Fact]
    public void Non_utf8_bytes_are_decoded_with_the_declared_charset()
    {
        // 0xA4 is the euro sign in iso-8859-15 (but the currency sign in iso-8859-1), 0xFC is ü
        byte[] bytes = [.. "<meta charset=\"iso-8859-15\"><p>5 "u8, 0xA4, .. " f"u8, 0xFC, .. "r M"u8, 0xFC, .. "nzen</p>"u8];

        Assert.Contains("5 € für Münzen", HtmlDecoding.Decode(bytes), StringComparison.Ordinal);
    }
}
