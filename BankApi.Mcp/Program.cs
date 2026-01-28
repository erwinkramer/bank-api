using System.Security.Claims;
using MCPify.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var tenantId = "b81eb003-1c5c-45fd-848f-90d9d3f8d016";
var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMCPService(tenantId, builder.Configuration["ApiBaseUrl"]!, "v1", builder.Configuration["McpServerBaseUrl"]!);

builder.Services.PostConfigure<JwtBearerOptions>(
    JwtBearerDefaults.AuthenticationScheme, options =>
{
    // Configure to validate tokens from Microsoft Entra ID
    options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuers = new[] { 
            $"https://login.microsoftonline.com/{tenantId}/v2.0",
            $"https://sts.windows.net/{tenantId}/"
        }
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var name = context.Principal?.FindFirstValue("name") ?? 
                      context.Principal?.FindFirstValue("preferred_username") ?? "unknown";
            var upn = context.Principal?.FindFirstValue("upn") ??
                       context.Principal?.FindFirstValue("email") ?? 
                       context.Principal?.FindFirstValue("preferred_username") ?? "unknown";
            var tenantId = context.Principal?.FindFirstValue("tid");
            Console.WriteLine($"Token validated for: {name} ({upn}) from tenant: {tenantId}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"Challenging client to authenticate with Entra ID");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcpifyEndpoint(); // Map the MCP Endpoint (for HTTP transport) and OAuth metadata endpoint
app.MapAuthCallback("/auth/callback"); // only required when not using the official MCP server authorization flow

// Register MCP Tools (Must be called after endpoints are mapped but before Run)
var registrar = app.Services.GetRequiredService<McpifyServiceRegistrar>();
await registrar.RegisterToolsAsync(((IEndpointRouteBuilder)app).DataSources);

app.Run();
