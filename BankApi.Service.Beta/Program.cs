using System.Security.Cryptography;
using Gridify;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var ecSigner = ECDsa.Create(ECCurve.NamedCurves.nistP521); // Typically, you'd load this from a secure location and not create a new one each time

GlobalConfiguration.ApiDocument = builder.Configuration.GetRequiredSection("ApiDocument").Get<OpenApiDocument>()!;
GlobalConfiguration.ApiSettings = builder.Configuration.GetRequiredSection("ApiSettings").Get<GlobalConfiguration.SettingsModel>()!;
GlobalConfiguration.ApiExamples = OpenApiFactory.CreateFromJson(File.ReadAllText("./appexamples.json"))!;

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
builder.Services.AddCorsServices();
builder.Services.AddErrorHandling();

var app = builder.Build();

app.UseMiddleware<JwsResponseSigningMiddleware>(ecSigner);
app.UseMiddleware<ApiVersionHeaderMiddleware>();
app.UseExceptionHandler();
app.UsePathBase(new($"/{GlobalConfiguration.ApiDocument.Info.Version}")); // Useful when versioning routing happens in an API Management system
app.UseAuthorization(); // explicitly register because we use path base
app.UseMiddleware<EntraIdTokenReuseMiddleware>(); // needs to be at least after authorization
app.UseRateLimiter();
app.UseCors();

app.MapOpenApi("/openapi/{documentName}.json");

if (app.Environment.IsDevelopment())
{
    app.AddOpenApiScalarReference();
    await app.Services.ProvisionAzureStorage();
}

app.Services.EnsureDataServicesCreated();

app.MapBankEndpoints();
app.MapTellerEndpoints();
app.MapHealthChecks("/health").RequireAuthorization("bank_subscription");

app.Run();
