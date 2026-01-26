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
    public static async Task<IServiceCollection> AddMCPService(this IServiceCollection services, string apiBaseUrl, string? apiVersion, string mcpServerBaseUrl)
    {
        var openApiPath = Path.Combine(AppContext.BaseDirectory, $"openapi_{apiVersion}.json");
        var tenantId = "b81eb003-1c5c-45fd-848f-90d9d3f8d016";

        // only required when not using the official MCP server authorization flow
        // services.AddScoped(sp =>
        // {
        //     return new OAuthAuthorizationCodeAuthentication(
        //         clientId: "b6997777-3799-4c55-b78a-4ce96e3d959c",
        //         clientSecret: "fQB8Q~GkKsBaQFKnrTLEGXpRHWejyASJB6ZMGba~",
        //         authorizationEndpoint: $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize",
        //         tokenEndpoint: $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
        //         scope: "b6997777-3799-4c55-b78a-4ce96e3d959c/.default",
        //         secureTokenStore: sp.GetRequiredService<ISecureTokenStore>(),
        //         mcpContextAccessor: sp.GetRequiredService<IMcpContextAccessor>(),
        //         redirectUri: $"{mcpServerBaseUrl}/auth/callback",
        //         usePkce: true
        //     );
        // });

        services.AddScoped(sp =>
        {
            return new ApiKeyAuthentication(
                keyName: "Ocp-Apim-Subscription-Key",
                keyValue: "Lifetime Subscription"
            );
        });

        services.AddMcpify(options =>
        {
            options.ResourceUrlOverride = mcpServerBaseUrl;
            options.Transport = McpTransportType.Http;
            //options.TokenValidation = new TokenValidationOptions
            //{
            //    EnableJwtValidation = true,
            //    ValidateAudience = true
            //};
            options.OAuthConfigurations.Add(new()
            {
                AuthorizationServers = [$"https://login.microsoftonline.com/{tenantId}/v2.0"]
            });
            options.ExternalApis.Add(new()
            {
                OpenApiFilePath = openApiPath,
                ApiBaseUrl = $"{apiBaseUrl}/{apiVersion}",
                //Filter = op => op.Route.StartsWith("/banks"),
                ToolPrefix = "bankApi",
                AuthenticationFactory = sp => sp.GetRequiredService<ApiKeyAuthentication>()
            });
        });

        //services.AddSingleton<McpServerTool>(_ => new LoginTool());

        return services;
    }
}