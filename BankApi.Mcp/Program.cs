using MCPify.Hosting;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMCPService("v1");

var app = builder.Build();

// 3. Register MCP Tools (Must be called after endpoints are mapped but before Run)
var registrar = app.Services.GetRequiredService<McpifyServiceRegistrar>();
await registrar.RegisterToolsAsync(((IEndpointRouteBuilder)app).DataSources);

// 4. Map the MCP Endpoint
app.MapMcpifyEndpoint(); 

app.Run();
