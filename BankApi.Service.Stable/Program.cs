using System.Text.Json;
using System.Text.Json.Nodes;
using Gridify;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

GlobalConfiguration.ApiDocument = builder.Configuration.GetRequiredSection("ApiDocument").Get<OpenApiDocument>()!;
GlobalConfiguration.ApiSettings = builder.Configuration.GetRequiredSection("ApiSettings").Get<GlobalConfiguration.SettingsModel>()!;
GlobalConfiguration.ApiExamples = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText("./appexamples.json"))!;

GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
GridifyGlobalConfiguration.DefaultPageSize = GlobalConfiguration.ApiSettings.PageSize.Default;

builder.AddLoggingServices();
builder.AddComplianceServices();
builder.AddAzureClients();
builder.Services.ConfigureJson();
builder.Services.AddHealthChecks();
builder.Services.AddAuthServices(builder.Environment);
builder.Services.AddDataServices();
builder.Services.AddDownstreamApiServices();
builder.Services.AddOpenApiServices();
builder.Services.AddRateLimitServices();
builder.Services.AddCorsServices();
builder.Services.AddErrorHandling();
builder.Services.AddValidation(); // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0#enable-built-in-validation-support-for-minimal-apis

var app = builder.Build();

app.MapJwk(out var jwk); // register JWKS endpoint
app.UseMiddleware<JwsResponseSigningMiddleware>(jwk);
app.UseMiddleware<ApiVersionHeaderMiddleware>();
app.UseExceptionHandler();
app.UsePathBase(new($"/{GlobalConfiguration.ApiDocument.Info.Version}")); // Useful when versioning routing happens in an API Management system
app.UseAuthorization(); // explicitly register because we use path base
app.UseMiddleware<EntraIdTokenReuseMiddleware>(); // needs to be at least after authorization
app.UseRateLimiter();
app.UseCors();

app.MapOpenApi("/openapi/{documentName}.json");

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.AddOpenApiScalarReference();
    await app.Services.ProvisionAzureStorage();
}

app.Services.EnsureDataServicesCreated();

app.MapBankEndpoints();
app.MapTellerEndpoints();
app.MapHealthChecks("/health").RequireAuthorization("bank_subscription");

app.Run();
