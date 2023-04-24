using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;

var uri = new Uri("https://localhost:9200");
var userName = "elastic";
Console.WriteLine($"Please enter the password for the `{userName}` user on {uri}");
var password = Console.ReadLine();

var settings = new ElasticsearchClientSettings(uri)
    .ServerCertificateValidationCallback((_, _, _, _) => true) // Bypass SSL certificate validation
    // .CertificateFingerprint("<FINGERPRINT>") // Alternatively, setup the self-signed cert fingerprint here
    .Authentication(new BasicAuthentication(userName, password));

var client = new ElasticsearchClient(settings);

Console.WriteLine("Start typing a title name, ESC to quit");
var input = "";
while (true)
{
    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.Escape)
    {
        break;
    }
    else if (key.Key == ConsoleKey.Backspace && input.Length >= 1)
    {
        input = input.Substring(0, input.Length - 1);
    }
    else
    {
        input += key.KeyChar;
    }
    Console.WriteLine("-----------------------------------------------");
    Console.WriteLine($"*** Multi-match bool prefix on {input}");
    await PrintAutoCompleteMultiMatch(client, input);
    Console.WriteLine($"*** Match phrase prefix on {input}");
    await PrintAutoCompleteMatchPhrase(client, input);
}

async Task PrintAutoCompleteMultiMatch(ElasticsearchClient client, string input)
{
    var response = await client.SearchAsync<Movie>(s => s
        .Index("movies_auto_complete")
        .From(0)
        .Size(10)
        .Query(q => q
            .MultiMatch(mmq => mmq
                .Query(input)
                .Type(TextQueryType.BoolPrefix)
                .Fields(new[] { "title", "title._2gram", "title._3gram" }))));

    PrintHits(response);
}

async Task PrintAutoCompleteMatchPhrase(ElasticsearchClient client, string input)
{
    var response = await client.SearchAsync<Movie>(s => s
        .Index("movies_auto_complete")
        .From(0)
        .Size(10)
        .Query(q => q
            .MatchPhrasePrefix(mpp => mpp
                .Query(input)
                .Field(m => m.Title))));

    PrintHits(response);
}

static void PrintHits(SearchResponse<Movie> response)
{
    foreach (var hit in response.Hits)
    {
        Console.WriteLine($"\t{hit.Score}\t{hit?.Source?.Title}");
    }
}

public class Movie
{
    public string? Title { get; set; }
    public string[]? Genres { get; set; }
    public int Id { get; set; }
    public int Year { get; set; }
}
