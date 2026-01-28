using AspNetCore.Authentication.ApiKey;
using Scalar.AspNetCore;

public static partial class ApiBuilder
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        services.AddOpenApi(GlobalConfiguration.ApiDocument!.Info.Version!, options =>
        {
            // Purposely set the spec to a version that is supported by Azure APIM, 
            // the 'Specs.Generated' output files still use the latest spec versions.
            options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0; 

            options.AddDocumentTransformer<TransformerDocInfo>();
            options.AddDocumentTransformer<TransformerComponentSchemas>();
            options.AddDocumentTransformer<TransformerComponentHeaders>();
            options.AddDocumentTransformer<TransformerComponentResponses>();
            options.AddDocumentTransformer<TransformerSecurityScheme>();
            options.AddSchemaTransformer<TransformerExampleSchema>();
            options.AddSchemaTransformer<TransformerGuidSchema>();
            options.AddOperationTransformer<TransformerOperation>();
        });

        return services;
    }

    public static void AddOpenApiScalarReference(this IEndpointRouteBuilder app)
    {
        app.MapScalarApiReference(options =>
        {
            options.Theme = ScalarTheme.DeepSpace;
            options.ForceDarkMode();
            options.AddApiKeyAuthentication($"{ApiKeyDefaults.AuthenticationScheme}-Header", options =>
            {
                options.Value = "Lifetime Subscription";
            });
            options.AddClientCredentialsFlow("OAuth2", options =>
            {
                options.TokenUrl = $"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/token";
                options.ClientId = GlobalConfiguration.ApiSettings!.EntraId.ClientId;
                options.ClientSecret = "fQB8Q~GkKsBaQFKnrTLEGXpRHWejyASJB6ZMGba~";
                options.CredentialsLocation = CredentialsLocation.Body;
                options.SelectedScopes = new List<string>
                {
                    $"{GlobalConfiguration.ApiSettings!.EntraId.ClientId}/.default"
                };
            });
            options.AddAuthorizationCodeFlow("OAuth2", options =>
            {
                options.AuthorizationUrl = $"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/authorize";
                options.TokenUrl = $"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/token";
                options.RefreshUrl = $"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/token";
                options.ClientId = GlobalConfiguration.ApiSettings!.EntraId.ClientId;
                options.CredentialsLocation = CredentialsLocation.Body;
                options.SelectedScopes = new List<string>
                {
                    $"{GlobalConfiguration.ApiSettings!.EntraId.ClientId}/.default"
                };
                options.Pkce = Pkce.Sha256;
            });
            options.Title = $"{GlobalConfiguration.ApiDocument!.Info.Title} docs | {GlobalConfiguration.ApiDocument.Info.Version}";
        });
    }
}