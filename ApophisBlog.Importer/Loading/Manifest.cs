namespace ApophisBlog.Importer.Loading;

using System.Globalization;
using System.Text.RegularExpressions;

using ApophisBlog.Importer.Functional;

/// <summary>One file in <c>content/</c> with the Wayback capture it came from.</summary>
public sealed record ManifestEntry(string LocalFile, DateTimeOffset CapturedAt, string OriginalUrl, string MimeType);

/// <summary>Parses <c>content-manifest.csv</c> (columns: local, timestamp, original, mimetype).</summary>
public static partial class Manifest
{
    public static Either<string, IReadOnlyList<ManifestEntry>> Parse(string csv)
        => csv.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Select((line, index) => (Line: line, Number: index + 1))
            .Skip(1)
            .Where(row => row.Line.Length > 0)
            .Select(row => ParseRow(row.Line, row.Number))
            .Sequence()
            .Select(LastRowPerFile);

    private static IReadOnlyList<ManifestEntry> LastRowPerFile(IReadOnlyList<ManifestEntry> entries)
        => entries.GroupBy(entry => entry.LocalFile).Select(rows => rows.Last()).ToList();

    private static Either<string, ManifestEntry> ParseRow(string line, int number)
        => Fields(line) switch
        {
            [var local, var timestamp, var original, var mime] => ParseTimestamp(timestamp)
                .ToEither($"line {number}: invalid timestamp '{timestamp}'")
                .Select(capturedAt => new ManifestEntry(local, capturedAt, original, mime)),
            var fields => Either<string, ManifestEntry>.Left($"line {number}: expected 4 fields, got {fields.Count}"),
        };

    private static List<string> Fields(string line)
        => CsvField().Matches(line)
            .Select(m => m.Groups["quoted"].Success ? m.Groups["quoted"].Value.Replace("\"\"", "\"", StringComparison.Ordinal) : m.Groups["plain"].Value)
            .ToList();

    private static Option<DateTimeOffset> ParseTimestamp(string timestamp)
        => DateTimeOffset.TryParseExact(timestamp, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value)
            ? value
            : Option<DateTimeOffset>.None;

    [GeneratedRegex("""(?:^|,)(?:"(?<quoted>(?:[^"]|"")*)"|(?<plain>[^,]*))""")]
    private static partial Regex CsvField();
}
