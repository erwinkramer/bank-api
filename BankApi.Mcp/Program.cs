using MCPify.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMCPService(builder.Configuration["ApiBaseUrl"]!, "v1", builder.Configuration["McpServerBaseUrl"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development, we accept any valid token without validating the signature (via the authority)
            // this is to simplify local development with self-signed tokens
            options.TokenValidationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);
        }
    });

builder.Services.AddAuthorization(options =>
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .Build());

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcpifyEndpoint(); // Map the MCP Endpoint (for HTTP transport) and OAuth metadata endpoint
app.MapAuthCallback("/auth/callback"); // only required when not using the official MCP server authorization flow

// Register MCP Tools (Must be called after endpoints are mapped but before Run)
var registrar = app.Services.GetRequiredService<McpifyServiceRegistrar>();
await registrar.RegisterToolsAsync(((IEndpointRouteBuilder)app).DataSources);

app.Run();
