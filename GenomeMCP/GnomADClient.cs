using System.Net.Http.Json;

namespace GenomeMCP;

public class GnomadClient : IGnomadClient
{
    private readonly HttpClient _httpClient;

    public GnomadClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ExecuteQueryAsync(string graphqlQuery, CancellationToken ct = default)
    {
        // Simply wrap the raw string in an anonymous object
        var requestBody = new { query = graphqlQuery };

        var response = await _httpClient.PostAsJsonAsync("", requestBody, ct);
        var responseContent = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            return $"GraphQL API Error ({response.StatusCode}): {responseContent}";
        }

        return responseContent;
    }
}