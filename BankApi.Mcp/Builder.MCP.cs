using MCPify.Core;
using MCPify.Core.Auth.OAuth;
using MCPify.Hosting;

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

        services.AddMcpify(options =>
        {
            options.Transport = McpTransportType.Http;
            
            options.ExternalApis.Add(new()
            {
                SwaggerFilePath = openApiPath,
               
                ApiBaseUrl = $"https://localhost:5201/{apiVersion}",
                ToolPrefix = "bankapi",
                Authentication = new OAuthAuthorizationCodeAuthentication(
                    clientId: "b6997777-3799-4c55-b78a-4ce96e3d959c",
                    authorizationEndpoint: "https://login.microsoftonline.com/b81eb003-1c5c-45fd-848f-90d9d3f8d016/oauth2/v2.0/authorize",
                    tokenEndpoint: "https://login.microsoftonline.com/b81eb003-1c5c-45fd-848f-90d9d3f8d016/oauth2/v2.0/token",
                    scope: "b6997777-3799-4c55-b78a-4ce96e3d959c/.default",
                    tokenStore: new FileTokenStore("token.json") // Persist token to disk
                )
            });
        });

        return services;
    }
}