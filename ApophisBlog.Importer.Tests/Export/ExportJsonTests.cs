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

    [Fact]
    public void Optional_headline_is_written_when_present()
    {
        var translation = new Translation(Language.De, "Chaos pur", "chaos-pur", Headline: Option.Some("Was gibt es aus Indien zu erzählen"));

        var json = JsonNode.Parse(ExportJson.Serialize(PostWith(translation)))!;

        Assert.Equal("Was gibt es aus Indien zu erzählen", json["translations"]![0]!["headline"]!.GetValue<string>());
    }

    [Fact]
    public void Optional_headline_is_left_out_when_absent()
    {
        var translation = new Translation(Language.De, "Chaos pur", "chaos-pur");

        var json = JsonNode.Parse(ExportJson.Serialize(PostWith(translation)))!;

        Assert.False(json["translations"]![0]!.AsObject().ContainsKey("headline"));
    }

    private static ExportPost PostWith(Translation translation)
        => new(Id: "journey/chaos-pur", Blog: "journey", Translations: [translation]);

    private static void AssertJsonEqual(string expected, string actual)
        => Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual)),
            $"Expected:{Environment.NewLine}{expected}{Environment.NewLine}Actual:{Environment.NewLine}{actual}");
}
