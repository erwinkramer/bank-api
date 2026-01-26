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

        services.AddScoped(sp =>
        {
            return new OAuthAuthorizationCodeAuthentication(
                clientId: "",
                authorizationEndpoint: "",
                tokenEndpoint: "",
                scope: "",
                secureTokenStore: null,
                mcpContextAccessor: null
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
                Filter = op => op.Route.StartsWith("/teller"),
                ToolPrefix = "bankApiViaOAuth",
                AuthenticationFactory = sp => sp.GetRequiredService<OAuthAuthorizationCodeAuthentication>()
            });
            options.ExternalApis.Add(new()
            {
                OpenApiFilePath = openApiPath,
                ApiBaseUrl = $"{apiBaseUrl}/{apiVersion}",
                Filter = op => op.Route.StartsWith("/banks"),
                ToolPrefix = "bankApiViaApiKey",
                AuthenticationFactory = sp => sp.GetRequiredService<ApiKeyAuthentication>()
            });
        });

        //services.AddSingleton<McpServerTool>(_ => new LoginTool());

        return services;
    }
}