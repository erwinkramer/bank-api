using MCPify.Core;
using MCPify.Core.Auth;
using MCPify.Core.Auth.OAuth;
using MCPify.Hosting;

public static partial class ApiBuilder
{
    /// <summary>
    /// Add MCP Service to the IServiceCollection.
    /// </summary>
    public static async Task<IServiceCollection> AddMCPService(this IServiceCollection services, string tenantId, string apiBaseUrl, string? apiVersion, string mcpServerBaseUrl)
    {
        var openApiPath = Path.Combine(AppContext.BaseDirectory, $"openapi_{apiVersion}.json");

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
                TokenSource = TokenSource.Client,
                ToolPrefix = "bankApiViaOAuth"
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

        return services;
    }
}