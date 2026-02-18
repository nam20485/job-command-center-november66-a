using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ProfileGenie.Harvester;

/// <summary>
/// Entry point for the Harvester worker service.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure services
        // TODO: Add ChromeConnector, LinkedInScraper, Humanizer services

        var host = builder.Build();
        await host.RunAsync();
    }
}
