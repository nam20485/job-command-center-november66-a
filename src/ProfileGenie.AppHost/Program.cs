using Microsoft.Extensions.Hosting;

namespace ProfileGenie.AppHost;

/// <summary>
/// Entry point for the ProfileGenie application orchestrator.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // TODO: Configure Aspire orchestration when packages are available
        // For now, this is a placeholder for service orchestration

        var host = builder.Build();
        await host.RunAsync();
    }
}
