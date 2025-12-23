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
                redirectUri: "http://localhost:5200/auth/callback",
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
                SwaggerFilePath = openApiPath,
                ApiBaseUrl = $"https://localhost:5201/{apiVersion}",
                ToolPrefix = "bankApiViaOAuth",
                AuthenticationFactory = sp => sp.GetRequiredService<OAuthAuthorizationCodeAuthentication>()
            });
            options.ExternalApis.Add(new()
            {
                SwaggerFilePath = openApiPath,
                ApiBaseUrl = $"https://localhost:5201/{apiVersion}",
                ToolPrefix = "bankApiViaApiKey",
                AuthenticationFactory = sp => sp.GetRequiredService<ApiKeyAuthentication>()
            });
        });

        services.AddLoginTool(sp => new LoginTool());

        return services;
    }

    private static IServiceCollection AddLoginTool(this IServiceCollection services, Func<IServiceProvider, LoginTool> factory)
    {
        services.AddSingleton<McpServerTool>(sp => factory(sp));
        return services;
    }

    public static void MapAuthCallback(this WebApplication app, string callbackPath, params string[] alternatePaths)
    {
        var paths = new List<string> { callbackPath };
        paths.AddRange(alternatePaths ?? Array.Empty<string>());

        foreach (var path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            app.MapGet(path, async context =>
            {
                var code = context.Request.Query["code"].ToString();
                var state = context.Request.Query["state"].ToString();

                if (string.IsNullOrEmpty(code))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Missing code");
                    return;
                }

                if (string.IsNullOrEmpty(state))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Missing state");
                    return;
                }

                var auth = context.RequestServices.GetRequiredService<OAuthAuthorizationCodeAuthentication>();

                try
                {
                    // The HandleAuthorizationCallbackAsync now takes the signed state and extracts sessionId
                    await auth.HandleAuthorizationCallbackAsync(code, state, context.RequestAborted);

                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("""
                        <html>
                        <head><title>Login Successful</title></head>
                        <body style="font-family: sans-serif; text-align: center; padding-top: 50px;">
                            <h1 style="color: green;">Login Successful!</h1>
                            <p>You have successfully authenticated with MCPify.</p>
                            <p>You can now close this window and return to your application.</p>
                            <script>
                                setTimeout(function() { window.close(); }, 3000);
                            </script>
                        </body>
                        </html>
                        """);
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    if (app.Environment.IsDevelopment())
                    {
                        var stateDebug = state.Length > 10 ? state.Substring(0, 5) + "..." + state.Substring(state.Length - 5) : state;
                        await context.Response.WriteAsync($"Auth exchange failed: {ex.Message}. State: {stateDebug} (Len: {state.Length})");
                    }
                    else
                    {
                        await context.Response.WriteAsync("Auth exchange failed. Please check server logs.");
                    }
                }
            });
        }
    }
}