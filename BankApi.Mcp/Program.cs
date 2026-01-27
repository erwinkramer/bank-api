using MCPify.Hosting;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMCPService(builder.Configuration["ApiBaseUrl"]!, "v1", builder.Configuration["McpServerBaseUrl"]!);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcpifyEndpoint(); // Map the MCP Endpoint (for HTTP transport) and OAuth metadata endpoint
app.MapAuthCallback("/auth/callback"); // only required when not using the official MCP server authorization flow

// Register MCP Tools (Must be called after endpoints are mapped but before Run)
var registrar = app.Services.GetRequiredService<McpifyServiceRegistrar>();
await registrar.RegisterToolsAsync(((IEndpointRouteBuilder)app).DataSources);

app.Run();
