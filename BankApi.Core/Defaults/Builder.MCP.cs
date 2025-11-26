using QuickMCP.Authentication;
using QuickMCP.Builders;
using QuickMCP.Extensions;

public static partial class ApiBuilder
{
    /// <summary>
    /// Add MCP Service to the IServiceCollection. Note: This does not work yet.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static async Task<IServiceCollection> AddMCPService(this IServiceCollection services)
    {
        // Create and configure a server
        var serverInfoBuilder = McpServerInfoBuilder.ForOpenApi()
            .FromConfiguration("../Specs.Generated/openapi_v1.json") // make more dynamic later
            .WithBaseUrl("https://localhost:5201/v1") // make more dynamic later
            .AddDefaultHeader("User-Agent", "QuickMCP Client")
            .AddAuthentication(new ApiKeyAuthenticator("your-api-key", "X-API-Key", ApiKeyAuthenticator.ApiKeyLocation.Header));

        // Build server info
        var serverInfo = await serverInfoBuilder.BuildAsync();

        services.AddMcpServer()
              .WithQuickMCP(serverInfo)
              .WithStdioServerTransport();

        return services;
    }
}