namespace ApophisBlog.Importer.Loading;

using System.Text;

/// <summary>Turns archived bytes into text without trusting the declared charset blindly.</summary>
public static class HtmlDecoding
{
    public static string Decode(byte[] bytes)
        => Encoding.UTF8.GetString(bytes);
}
