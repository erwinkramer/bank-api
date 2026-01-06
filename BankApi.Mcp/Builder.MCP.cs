using MCPify.Core;
using MCPify.Core.Auth;
using MCPify.Core.Auth.OAuth;
using MCPify.Hosting;
using ModelContextProtocol.Server;
using MCPify.Tools;

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

        services.AddScoped(sp =>
        {
            return new OAuthAuthorizationCodeAuthentication(
                clientId: "b6997777-3799-4c55-b78a-4ce96e3d959c",
                clientSecret: "fQB8Q~GkKsBaQFKnrTLEGXpRHWejyASJB6ZMGba~", // we need this because when running in Claude and authenticating via our MCP server, it's not a SPA.
                authorizationEndpoint: "https://login.microsoftonline.com/b81eb003-1c5c-45fd-848f-90d9d3f8d016/oauth2/v2.0/authorize",
                tokenEndpoint: "https://login.microsoftonline.com/b81eb003-1c5c-45fd-848f-90d9d3f8d016/oauth2/v2.0/token",
                scope: "b6997777-3799-4c55-b78a-4ce96e3d959c/.default",
                secureTokenStore: sp.GetRequiredService<ISecureTokenStore>(),
                mcpContextAccessor: sp.GetRequiredService<IMcpContextAccessor>(),
                redirectUri: "https://bankapi-mcp-001-ctcahwhschgrdqb4.westeurope-01.azurewebsites.net/auth/callback",
                usePkce: true
            );
        });

        services.AddScoped(sp =>
        {
            return new ApiKeyAuthentication(
                keyName: "Ocp-Apim-Subscription-Key",
                keyValue: "Lifetime Subscription"
            );
        });

        services.AddMcpify(options =>
        {
            options.Transport = McpTransportType.Http;
            options.ExternalApis.Add(new()
            {
                OpenApiFilePath = openApiPath,
                ApiBaseUrl = $"https://bankapi-001-ffamb7fcbkcgdsg7.westeurope-01.azurewebsites.net/{apiVersion}",
                ToolPrefix = "bankApiViaOAuth",
                AuthenticationFactory = sp => sp.GetRequiredService<OAuthAuthorizationCodeAuthentication>()
            });
            options.ExternalApis.Add(new()
            {
                OpenApiFilePath = openApiPath,
                ApiBaseUrl = $"https://bankapi-001-ffamb7fcbkcgdsg7.westeurope-01.azurewebsites.net/{apiVersion}",
                ToolPrefix = "bankApiViaApiKey",
                AuthenticationFactory = sp => sp.GetRequiredService<ApiKeyAuthentication>()
            });
        });

        services.AddSingleton<McpServerTool>(_ => new LoginTool());

        return services;
    }
}