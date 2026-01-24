using MCPify.Hosting;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMCPService(builder.Configuration["ApiBaseUrl"]!, "v1", builder.Configuration["McpServerBaseUrl"]!);

var app = builder.Build();

app.UseMcpifyContext(); // Enable MCPify's context accessor middleware
app.UseMcpifyOAuth(); // Enable MCPify's OAuth middleware

app.MapMcpifyEndpoint(); // Map the MCP Endpoint (for HTTP transport) and OAuth metadata endpoint
app.MapAuthCallback("/auth/callback");

// Register MCP Tools (Must be called after endpoints are mapped but before Run)
var registrar = app.Services.GetRequiredService<McpifyServiceRegistrar>();
await registrar.RegisterToolsAsync(((IEndpointRouteBuilder)app).DataSources);

app.Run();
