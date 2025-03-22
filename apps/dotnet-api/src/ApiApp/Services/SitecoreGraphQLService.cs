using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Diagnostics;

namespace ApiApp.Services;

public record TestDataModel(string Title, string Content, TimeSpan Elapsed);

public class SitecoreGraphQLService
{
    private readonly GraphQLHttpClient _client;

    public SitecoreGraphQLService()
    {
        _client = new GraphQLHttpClient(options => { options.EndPoint = new Uri("http://cm/sitecore/api/graph/edge"); }, new SystemTextJsonSerializer());
        _client.HttpClient.DefaultRequestHeaders.Add("sc_apikey", "F98A130C-A45D-4231-AAEC-12BA185DF801");
    }

    public async Task<TestDataModel> GetDataAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = new GraphQLRequest
        {
            Query = """
                query {
                  item(path: "/sitecore/content/Home", language: "en") {
                    path
                    title: field(name: "Title") {
                      jsonValue
                    }
                    content: field(name: "Text") {
                      jsonValue
                    }
                  }

                  search(
                    where: {
                      name: "_path"
                      value: "{0DE95AE4-41AB-4D01-9EB0-67441B7C2450}"
                      operator: CONTAINS
                    }
                    first: 10
                  ) {
                    results {
                      name
                      id
                      path
                    }
                  }
                }
                """
        };

        var response = await _client.SendQueryAsync<ItemResponse>(request, cancellationToken);

        return new TestDataModel(response.Data.Item.Title.JsonValue.Value, response.Data.Item.Content.JsonValue.Value, stopwatch.Elapsed);
    }

    private record JsonValue(string Value);

    private record Field(JsonValue JsonValue);

    private record Item(string Path, Field Title, Field Content);

    private record ItemResponse(Item Item);
}
