var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMCPService("v1");

var app = builder.Build();

app.MapMcp("/mcp/v1");

app.Run();
