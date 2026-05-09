namespace GenomeMCP;

public interface IGnomadClient
{
    /// <summary>
    /// Executes a raw GraphQL query against the gnomAD API.
    /// </summary>
    /// <param name="graphqlQuery">The raw GraphQL query string.</param>
    /// <param name="variables">Optional JSON string of variables.</param>
    /// <returns>The raw JSON response from the API.</returns>
    Task<string> ExecuteQueryAsync(
        string graphqlQuery,
        CancellationToken ct = default);
}
