namespace GenomeMCP;

[McpServerToolType]
public class GnomADTools(GnomADMcpService service)
{
    private readonly GnomADMcpService _service = service;

    [McpServerTool]
    [Description(
        "Explores the gnomAD GraphQL schema. Use this BEFORE executing queries to ensure you have the right fields. " +
        "If typeName is empty, it returns the root queries (gene, variant, etc.). " +
        "If typeName is provided (e.g., 'Gene', 'VariantDetails'), it returns the fields for that specific type.")]
    public async Task<string> IntrospectGnomadSchema(
        [Description("Optional: The GraphQL type to inspect (e.g., 'Gene'). Leave empty for root queries.")] string? typeName = null)
    {
        try
        {
            string query;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                // Query to get root level operations
                // (e.g., gene, transcript, variant_search, etc.)
                query = @"{ 
                  __schema { 
                    queryType { 
                      fields { 
                        name 
                        description 
                        args { name type { name kind ofType { name kind } } } 
                      } 
                    } 
                  } 
                }";
            }
            else
            {
                // Query to inspect a specific type's fields
                // (e.g., what fields are on the 'Gene' type?)
                query = $@"{{ 
                  __type(name: ""{typeName}"") {{ 
                    name 
                    fields {{ 
                      name 
                      description 
                      type {{ name kind ofType {{ name kind }} }} 
                    }} 
                  }} 
                }}";
            }

            return await _service.ExecuteRawQueryAsync(query);
        }
        catch (Exception ex)
        {
            return $"Introspection failed. Error: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description(
        "Executes a well-formed GraphQL query against gnomAD. " +
        "CRITICAL RULE: DO NOT request massive arrays (like all variants for a gene) unless you apply strict limits. " +
        "If the response is too large, refine your GraphQL query instead of writing local PowerShell/Bash scripts to analyze the output.")]
    public async Task<string> ExecuteGnomadQuery(
        [Description("The raw GraphQL query string. Do not use GraphQL variables, inject arguments directly into the string.")] string query)
    {
        try
        {
            return await _service.ExecuteRawQueryAsync(query);
        }
        catch (Exception ex)
        {
            return $"Query execution failed. Error: {ex.Message}";
        }
    }
}