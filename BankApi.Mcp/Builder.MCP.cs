using QuickMCP.Authentication;
using QuickMCP.Builders;
using QuickMCP.Extensions;

public static partial class ApiBuilder
{
    /// <summary>
    /// Add MCP Service to the IServiceCollection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    public static async Task<IServiceCollection> AddMCPService(this IServiceCollection services, string? apiVersion)
    {
        var openApiPath = Path.Combine(AppContext.BaseDirectory, $"openapi_{apiVersion}.json");

        // Create and configure a server
        // Doesn't work yet, see https://github.com/gunpal5/QuickMCP/issues/8
        var serverInfoBuilder = McpServerInfoBuilder.ForOpenApi("bankapi")
            .FromFile(openApiPath)
            .AddDefaultHeader("User-Agent", "QuickMCP Client")
            .AddAuthentication(new ApiKeyAuthenticator("Lifetime Subscription", "Ocp-Apim-Subscription-Key", ApiKeyAuthenticator.ApiKeyLocation.Header));

        // Build server info
        var serverInfo = await serverInfoBuilder.BuildAsync();
        Console.WriteLine($"QuickMCP built {serverInfo.ToolCount} tools for {apiVersion}");

        services.AddMcpServer()
              .WithQuickMCP(serverInfo)
              .WithHttpTransport();

        return services;
    }
}