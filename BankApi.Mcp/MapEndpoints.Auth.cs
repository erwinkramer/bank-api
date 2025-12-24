using MCPify.Core.Auth.OAuth;

public static partial class ApiMapper
{
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