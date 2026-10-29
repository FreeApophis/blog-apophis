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
            Author: "Apophis",
            Blog: "journey",
            Translations: [new Translation(Language.En, Title: "Hello 香港 (Hongkong)", Slug: "hello-hongkong", Body: new FullBody("<p>Hello</p>"))]);

        AssertJsonEqual(
            """
            {
              "kind": "post",
              "id": "journey/hello-hongkong",
              "author": "Apophis",
              "blog": "journey",
              "translations": [
                {
                  "language": "en",
                  "title": "Hello 香港 (Hongkong)",
                  "slug": "hello-hongkong",
                  "tags": [],
                  "sources": [],
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

    [Fact]
    public void Tags_are_written_per_translation()
    {
        var translation = new Translation(Language.De, "Chaos pur", "chaos-pur", AnyBody) { Tags = ["Indien", "Verkehr"] };

        var tags = SerializedTranslation(translation)["tags"]!.AsArray().Select(t => t!.GetValue<string>());

        Assert.Equal(["Indien", "Verkehr"], tags);
    }

    [Fact]
    public void Author_is_written_on_the_item()
    {
        var post = new ExportPost(Id: "journey/any", Author: "Apophis", Blog: "journey", Translations: [new Translation(Language.En, "t", "t", AnyBody)]);

        Assert.Equal("Apophis", JsonNode.Parse(ExportJson.Serialize(post))!["author"]!.GetValue<string>());
    }

    [Fact]
    public void Page_is_serialized_with_kind_page_and_without_blog()
    {
        var page = new ExportPage(Id: "pages/impressum", Author: "Apophis", Translations: [new Translation(Language.De, "Impressum", "impressum", AnyBody)]);

        var json = JsonNode.Parse(ExportJson.Serialize(page))!.AsObject();

        Assert.Equal("page", json["kind"]!.GetValue<string>());
        Assert.False(json.ContainsKey("blog"));
    }

    [Fact]
    public void Sources_record_where_each_copy_came_from()
    {
        var source = new Source(
            SourceSystem.Drupal,
            OriginalUrl: "http://apophis.eu/node/87",
            LocalFile: "apophis.eu/node/87/index.html",
            CapturedAt: new DateTimeOffset(2011, 6, 19, 3, 4, 5, TimeSpan.Zero),
            PublishedAt: new DateTimeOffset(2008, 7, 14, 0, 0, 0, TimeSpan.FromHours(2)));
        var translation = new Translation(Language.En, "Pure Chaos", "pure-chaos", AnyBody) { Sources = [source] };

        AssertJsonEqual(
            """
            [
              {
                "system": "drupal",
                "originalUrl": "http://apophis.eu/node/87",
                "localFile": "apophis.eu/node/87/index.html",
                "capturedAt": "2011-06-19T03:04:05+00:00",
                "publishedAt": "2008-07-14T00:00:00+02:00"
              }
            ]
            """,
            SerializedTranslation(translation)["sources"]!.ToJsonString());
    }

    private static readonly Body AnyBody = new FullBody("<p>text</p>");

    private static JsonNode SerializedTranslation(Translation translation)
        => JsonNode.Parse(ExportJson.Serialize(new ExportPost(Id: "journey/any", Author: "Apophis", Blog: "journey", Translations: [translation])))!["translations"]![0]!;

    private static void AssertJsonEqual(string expected, string actual)
        => Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual)),
            $"Expected:{Environment.NewLine}{expected}{Environment.NewLine}Actual:{Environment.NewLine}{actual}");
}
