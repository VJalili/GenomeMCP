using System.Text.Json;

namespace GenomeMCP;

public static class McpJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static readonly JsonSerializerOptions Compact = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
