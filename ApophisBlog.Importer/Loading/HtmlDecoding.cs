namespace ApophisBlog.Importer.Loading;

using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;

/// <summary>Turns archived bytes into text without trusting the declared charset blindly.</summary>
/// <remarks>
/// Valid UTF-8 is UTF-8, whatever the page claims (texts.php declares iso-8859-1 but is UTF-8).
/// Otherwise the declared charset is used.
/// </remarks>
public static partial class HtmlDecoding
{
    private const int DeclarationSearchLength = 4096;

    public static string Decode(byte[] bytes)
        => Utf8.IsValid(bytes)
            ? Encoding.UTF8.GetString(bytes)
            : DeclaredEncoding(bytes).GetOrElse(Encoding.Latin1).GetString(bytes);

    private static Option<Encoding> DeclaredEncoding(byte[] bytes)
        => CharsetDeclaration()
            .Match(Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, DeclarationSearchLength)))
            .ToOption()
            .SelectMany(match => EncodingByName(match.Groups["name"].Value));

    private static Option<Encoding> EncodingByName(string name)
        => Option.FromNullable(CodePagesEncodingProvider.Instance.GetEncoding(name))
            .OrElse(() => Encoding.GetEncodings()
                .FirstOrNone(info => string.Equals(info.Name, name, StringComparison.OrdinalIgnoreCase))
                .Select(info => info.GetEncoding()));

    private static Option<Match> ToOption(this Match match)
        => match.Success ? match : Option<Match>.None;

    [GeneratedRegex("""charset\s*=\s*["']?(?<name>[\w.:-]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex CharsetDeclaration();
}
