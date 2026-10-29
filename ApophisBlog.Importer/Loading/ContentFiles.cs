namespace ApophisBlog.Importer.Loading;

/// <summary>A manifest entry together with its decoded file content.</summary>
public sealed record LoadedFile(ManifestEntry Entry, string Text);

/// <summary>Reads files listed in the manifest from the <c>content/</c> folder (the IO edge of the importer).</summary>
public static class ContentFiles
{
    public static Either<string, LoadedFile> Load(string contentRoot, ManifestEntry entry)
    {
        var path = Path.Combine([contentRoot, .. entry.LocalFile.Split('/')]);
        try
        {
            return new LoadedFile(entry, HtmlDecoding.Decode(File.ReadAllBytes(path)));
        }
        catch (IOException exception)
        {
            return Either<string, LoadedFile>.Left($"{entry.LocalFile}: {exception.Message}");
        }
    }
}
