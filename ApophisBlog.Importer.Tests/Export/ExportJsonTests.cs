namespace ApophisBlog.Importer.Tests.Export;

using System.Text.Json.Nodes;

using ApophisBlog.Importer.Export;

public sealed class ExportJsonTests
{
    [Fact]
    public void Post_is_serialized_with_kind_discriminator_and_camel_case_fields()
    {
        var post = new ExportPost(
            Id: "journey/hello-hongkong",
            Blog: "journey",
            Translations: [new Translation(Language.En, Title: "Hello 香港 (Hongkong)", Slug: "hello-hongkong")]);

        AssertJsonEqual(
            """
            {
              "kind": "post",
              "id": "journey/hello-hongkong",
              "blog": "journey",
              "translations": [
                { "language": "en", "title": "Hello 香港 (Hongkong)", "slug": "hello-hongkong" }
              ]
            }
            """,
            ExportJson.Serialize(post));
    }

    private static void AssertJsonEqual(string expected, string actual)
        => Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual)),
            $"Expected:{Environment.NewLine}{expected}{Environment.NewLine}Actual:{Environment.NewLine}{actual}");
}
