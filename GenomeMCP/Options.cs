using System.Text.Json;

namespace GenomeMCP;

public class Options
{
    public string WorkingDir { init; get; } = _wd;

    public LoggerOptions Logger { init; get; } =
        new()
        {
            // The `_` before `.log` is added to separate RepoName from a 
            // timestamp that serilog adds for each rolling file.
            LogFilename = Path.Join(_wd, $"{new LoggerOptions().RepoName}_.log")
        };

    public const char CsvDelimiter = '\t';

    private static readonly long _timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
    private static readonly string _wd = Path.Join(Environment.CurrentDirectory, $"session_{_timestamp}");

    public static JsonSerializerOptions JsonSerializationOptions
    {
        get
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }
    }
}
