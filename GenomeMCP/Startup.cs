using Microsoft.Extensions.Configuration;
using Serilog;

namespace GenomeMCP;

public class Startup
{
    public static HostBuilder GetHostBuilder(Options options)
    {
        var hostBuilder = new HostBuilder();

        var logFilename = options.Logger.LogFilename;

        Log.Logger =
            new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override(
                "System.Net.Http.HttpClient",
                Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: logFilename,
                rollingInterval: RollingInterval.Hour,
                outputTemplate: options.Logger.MessageTemplate,
                shared: true,
                retainedFileCountLimit: null)
            /*.WriteTo.Console(
                theme: AnsiConsoleTheme.Code)*/
            .CreateLogger();
        hostBuilder.UseSerilog();

        hostBuilder.ConfigureAppConfiguration(
            (hostingContext, configuration) =>
            {
                ConfigureApp(hostingContext, configuration, options);
            });

        hostBuilder.ConfigureServices(
            services =>
            {
                ConfigureServices(services, options);
            });

        return hostBuilder;
    }

    private static void ConfigureApp(
        HostBuilderContext context,
        IConfigurationBuilder config,
        Options options)
    {
        config.Sources.Clear();
        var env = context.HostingEnvironment;

        config
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile(
                $"appsettings.json",
                optional: true,
                reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{env.EnvironmentName}.json",
                optional: true,
                reloadOnChange: true);

        var configRoot = config.Build();
        configRoot.GetSection(nameof(Options)).Bind(options);
    }

    private static void ConfigureServices(IServiceCollection services, Options options)
    {
        services.AddSingleton(options);

        // 1. Register the typed HttpClient for your Gnomad integration
        services.AddHttpClient<IGnomadClient, GnomadClient>(client =>
        {
            // We set the BaseAddress here so you don't have to hardcode it in the Client class
            client.BaseAddress = new Uri("https://gnomad.broadinstitute.org/api");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "GenomeMCP-Client/1.0");
        });

        // 2. Register the service so the Tool can resolve it via constructor injection
        services.AddTransient<GnomADMcpService>();

        // 3. Keep your existing MCP server setup. 
        // WithToolsFromAssembly() will now successfully build GnomADTools because
        // its dependencies (GnomADMcpService -> IGnomadClient -> HttpClient) are all registered!
        services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        // Suppress host lifetime messages from stdout
        services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);
    }
}
