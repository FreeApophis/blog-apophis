namespace ApophisBlog.Importer.Tests.Loading;

using System.Text;

using ApophisBlog.Importer.Loading;

public sealed class ContentFilesTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("apophis-content-");

    [Fact]
    public void Listed_file_is_loaded_and_decoded()
    {
        var entry = Entry("apophis.eu/node/87/index.html");
        WriteFile(entry.LocalFile, Encoding.UTF8.GetBytes("<h1>Pure Chaos</h1>"));

        FunctionalAssert.Right(new LoadedFile(entry, "<h1>Pure Chaos</h1>"), ContentFiles.Load(_root.FullName, entry));
    }

    [Fact]
    public void Missing_file_is_an_error_value()
        => FunctionalAssert.Left(ContentFiles.Load(_root.FullName, Entry("apophis.eu/node/999/index.html")));

    public void Dispose()
        => _root.Delete(recursive: true);

    private static ManifestEntry Entry(string localFile)
        => new(localFile, DateTimeOffset.UnixEpoch, "http://example/", "text/html");

    private void WriteFile(string localFile, byte[] bytes)
    {
        var path = Path.Combine(_root.FullName, localFile);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }
}
