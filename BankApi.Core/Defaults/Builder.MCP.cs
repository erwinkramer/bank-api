using QuickMCP.Authentication;
using QuickMCP.Builders;
using QuickMCP.Extensions;

public static partial class ApiBuilder
{
    /// <summary>
    /// Add MCP Service to the IServiceCollection. Note: This does not work yet, see https://github.com/gunpal5/QuickMCP/issues/7
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static async Task<IServiceCollection> AddMCPService(this IServiceCollection services, string? apiVersion)
    {
        // Create and configure a server
        var serverInfoBuilder = McpServerInfoBuilder.ForOpenApi()
            .FromConfiguration($"../Specs.Generated/openapi_{apiVersion}.json")
            .WithBaseUrl($"https://localhost:5201/{apiVersion}")
            .AddDefaultHeader("User-Agent", "QuickMCP Client")
            .AddAuthentication(new ApiKeyAuthenticator("your-api-key", "X-API-Key", ApiKeyAuthenticator.ApiKeyLocation.Header)); // modify this later

        // Build server info
        var serverInfo = await serverInfoBuilder.BuildAsync();

        services.AddMcpServer()
              .WithQuickMCP(serverInfo)
              .WithStdioServerTransport();

        return services;
    }
}