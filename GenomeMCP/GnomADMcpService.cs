using System.Text.Json;

namespace GenomeMCP;

public class GnomADMcpService(IGnomadClient gnomadClient)
{
    private readonly IGnomadClient _gnomadClient = gnomadClient;

    public async Task<string> ExecuteRawQueryAsync(string query)
    {
        var rawJson = await _gnomadClient.ExecuteQueryAsync(query);

        // 50,000 characters is roughly 10k to 15k tokens, so not to overwhelm the agent's context window.
        int maxAllowedLength = 50000;

        if (rawJson.Length > maxAllowedLength)
        {
            // The LLM will read this, realize it asked for too much, and refine its query.
            var errorResponse = new
            {
                errors = new[]
                {
                    new
                    {
                        message = $"PAYLOAD TOO LARGE: " +
                        $"The response was {rawJson.Length} characters, " +
                        $"which exceeds the maximum allowed limit of {maxAllowedLength}. " +
                        $"DO NOT write local scripts or PowerShell to bypass this. " +
                        $"You MUST refine your GraphQL query to request less data " +
                        $"(e.g., query specific variants instead of an entire gene)."
                    }
                }
            };

            return JsonSerializer.Serialize(errorResponse);
        }

        return rawJson;
    }
}