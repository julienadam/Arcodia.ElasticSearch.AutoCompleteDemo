using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using System.Diagnostics;

var uri = new Uri("https://localhost:9200");
var userName = "elastic";
Console.WriteLine($"Please enter the password for the `{userName}` user on {uri}");
var password = Console.ReadLine();

var settings = new ElasticsearchClientSettings(uri)
    .ServerCertificateValidationCallback((_, _, _, _) => true) // Bypass SSL certificate validation
    // .CertificateFingerprint("<FINGERPRINT>") // Alternatively, setup the self-signed cert fingerprint here
    .Authentication(new BasicAuthentication(userName, password));

var client = new ElasticsearchClient(settings);

var fieldToSearch = "name.search";

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
    var sw = Stopwatch.StartNew();
    await PrintAutoCompleteMultiMatch(client, input);
    Console.WriteLine($"Done in {sw.Elapsed}");
    sw.Restart();
    Console.WriteLine($"*** Match phrase prefix on {input}");
    await PrintAutoCompleteMatchPhrase(client, input);
    Console.WriteLine($"Done in {sw.Elapsed}");
}

async Task PrintAutoCompleteMultiMatch(ElasticsearchClient client, string input)
{
    var response = await client.SearchAsync<Card>(s => s
        .Index("mtg_v2")
        .From(0)
        .Size(10)
        .SourceIncludes(new[] { "name", "set" })
        .Query(q => q
            .MultiMatch(mmq => mmq
                .Query(input)
                .Type(TextQueryType.BoolPrefix)
                .Fields(new[] { fieldToSearch, $"{fieldToSearch}._2gram", $"{fieldToSearch}._3gram" }))));

    PrintHits(response);
}

async Task PrintAutoCompleteMatchPhrase(ElasticsearchClient client, string input)
{
    var response = await client.SearchAsync<Card>(s => s
        .Index("mtg_v2")
        .From(0)
        .Size(10)
        .SourceIncludes(new[] { "name", "set" })
        .Query(q => q
            .MatchPhrasePrefix(mpp => mpp
                .Query(input)
                .Field(fieldToSearch))));

    PrintHits(response);
}

static void PrintHits(SearchResponse<Card> response)
{
    foreach (var hit in response.Hits)
    {
        Console.WriteLine($"\t{hit?.Score}\t{hit?.Source?.Set}\t{hit?.Source?.Name}");
    }
}

public class Card
{
    public string? Name { get; set; }
    public string? Set { get; set; }
}