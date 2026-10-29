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
            Translations: [new Translation(Language.En, Title: "Hello 香港 (Hongkong)", Slug: "hello-hongkong", Body: new FullBody("<p>Hello</p>"))]);

        AssertJsonEqual(
            """
            {
              "kind": "post",
              "id": "journey/hello-hongkong",
              "blog": "journey",
              "translations": [
                {
                  "language": "en",
                  "title": "Hello 香港 (Hongkong)",
                  "slug": "hello-hongkong",
                  "body": { "completeness": "full", "html": "<p>Hello</p>" }
                }
              ]
            }
            """,
            ExportJson.Serialize(post));
    }

    [Fact]
    public void Optional_headline_is_written_when_present()
    {
        var translation = new Translation(Language.De, "Chaos pur", "chaos-pur", AnyBody, Headline: Option.Some("Was gibt es aus Indien zu erzählen"));

        var json = SerializedTranslation(translation);

        Assert.Equal("Was gibt es aus Indien zu erzählen", json["headline"]!.GetValue<string>());
    }

    [Fact]
    public void Optional_headline_is_left_out_when_absent()
    {
        var translation = new Translation(Language.De, "Chaos pur", "chaos-pur", AnyBody);

        var json = SerializedTranslation(translation);

        Assert.False(json.AsObject().ContainsKey("headline"));
    }

    [Fact]
    public void Summary_body_is_marked_as_summary()
    {
        var translation = new Translation(Language.En, "Namaste Neu Delhi", "namaste-neu-delhi", new SummaryBody("<p>It is hot…</p>"));

        var body = SerializedTranslation(translation)["body"]!;

        Assert.Equal("summary", body["completeness"]!.GetValue<string>());
        Assert.Equal("<p>It is hot…</p>", body["html"]!.GetValue<string>());
    }

    [Fact]
    public void Publication_date_is_iso_8601_with_offset()
    {
        var publishedAt = new DateTimeOffset(2008, 7, 5, 12, 51, 29, TimeSpan.FromHours(2));
        var translation = new Translation(Language.De, "Namaste Neu Delhi", "namaste-neu-delhi", AnyBody, PublishedAt: publishedAt);

        Assert.Equal("2008-07-05T12:51:29+02:00", SerializedTranslation(translation)["publishedAt"]!.GetValue<string>());
    }

    private static readonly Body AnyBody = new FullBody("<p>text</p>");

    private static JsonNode SerializedTranslation(Translation translation)
        => JsonNode.Parse(ExportJson.Serialize(new ExportPost(Id: "journey/any", Blog: "journey", Translations: [translation])))!["translations"]![0]!;

    private static void AssertJsonEqual(string expected, string actual)
        => Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual)),
            $"Expected:{Environment.NewLine}{expected}{Environment.NewLine}Actual:{Environment.NewLine}{actual}");
}
