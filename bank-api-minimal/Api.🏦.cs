using Gridify;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

GlobalConfiguration.ApiDocument = builder.Configuration.GetRequiredSection("ApiDocument").Get<OpenApiDocument>()!;
GlobalConfiguration.ApiSettings = builder.Configuration.GetRequiredSection("ApiSettings").Get<GlobalConfiguration.SettingsModel>()!;
GlobalConfiguration.ApiExamples = OpenApiAnyFactory.CreateFromJson(File.ReadAllText("./appexamples.json"))!;

GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
GridifyGlobalConfiguration.DefaultPageSize = GlobalConfiguration.ApiSettings.PageSize.Default;

builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

builder.Services.AddAuthServices();
builder.Services.AddDataServices();
builder.Services.AddDownstreamApiServices();
builder.Services.AddLoggingServices();
builder.Services.AddOpenApiServices();
builder.Services.AddRateLimitServices();

var app = builder.Build();

app.UsePathBase(new PathString($"/{GlobalConfiguration.ApiDocument.Info.Version}")); // Useful when versioning routing happens in an API Management system
app.UseAuthorization(); // explicitly register because we use path base
app.UseRateLimiter();

app.MapOpenApi("/openapi/{documentName}.json");

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        options.Theme = ScalarTheme.DeepSpace;
        options.WithApiKeyAuthentication(options =>
        {
            options.Token = "Lifetime Subscription";
        });
        options.Title = $"{GlobalConfiguration.ApiDocument.Info.Title} docs | {GlobalConfiguration.ApiDocument.Info.Version}";
    });
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BankDb>();
    dbContext.Database.EnsureCreated();
}

app.MapBankEndpoints();
app.MapTellerEndpoints();

app.Run();