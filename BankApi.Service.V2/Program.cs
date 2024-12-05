using Gridify;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

GlobalConfiguration.ApiDocument = builder.Configuration.GetRequiredSection("ApiDocument").Get<OpenApiDocument>()!;
GlobalConfiguration.ApiSettings = builder.Configuration.GetRequiredSection("ApiSettings").Get<GlobalConfiguration.SettingsModel>()!;
GlobalConfiguration.ApiExamples = OpenApiAnyFactory.CreateFromJson(File.ReadAllText("./appexamples.json"))!;

GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
GridifyGlobalConfiguration.DefaultPageSize = GlobalConfiguration.ApiSettings.PageSize.Default;

builder.AddLoggingServices();
builder.AddComplianceServices();
builder.AddAzureClients();
builder.Services.AddHealthChecks();
builder.Services.AddAuthServices();
builder.Services.AddDataServices();
builder.Services.AddDownstreamApiServices();
builder.Services.AddOpenApiServices();
builder.Services.AddRateLimitServices();

var app = builder.Build();

app.UsePathBase(new PathString($"/{GlobalConfiguration.ApiDocument.Info.Version}")); // Useful when versioning routing happens in an API Management system
app.UseAuthorization(); // explicitly register because we use path base
app.UseRateLimiter();

app.MapOpenApi("/openapi/{documentName}.json");

if (app.Environment.IsDevelopment())
{
    app.AddOpenApiScalarReference();
    app.Services.ProvisionAzureStorage();
}

app.Services.EnsureDataServicesCreated();

app.MapBankEndpoints();
app.MapTellerEndpoints();
app.MapHealthChecks("/health").RequireAuthorization("bank_subscription");

app.Run();
