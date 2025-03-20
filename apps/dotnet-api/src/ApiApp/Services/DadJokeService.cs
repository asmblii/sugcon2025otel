
namespace ApiApp.Services;

public class DadJoke
{
    public string Id { get; set; } = string.Empty;

    public string Joke { get; set; } = string.Empty;

    public int Status { get; set; }
}

public interface IDadJokeService
{
    Task<DadJoke> GetRandomJokeAsync();
    Task<DadJoke> GetJokeAsync(string id);
}

public class DadJokeService(HttpClient httpClient) : IDadJokeService
{
    private const string _randomJokeUrl = "/";

    public async Task<DadJoke> GetRandomJokeAsync() => await httpClient.GetFromJsonAsync<DadJoke>(_randomJokeUrl) ?? throw new Exception("Random joke not available from the desired service.");

    public async Task<DadJoke> GetJokeAsync(string id) => await httpClient.GetFromJsonAsync<DadJoke>($"/j/{id}") ?? throw new Exception($"Joke with id {id} not available from the desired service.");
}
