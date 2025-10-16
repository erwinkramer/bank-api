using AspNetCore.Authentication.ApiKey;
using Scalar.AspNetCore;

public static partial class ApiBuilder
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        services.AddOpenApi(GlobalConfiguration.ApiDocument!.Info.Version!, options =>
        {
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
            options.Title = $"{GlobalConfiguration.ApiDocument!.Info.Title} docs | {GlobalConfiguration.ApiDocument.Info.Version}";
        });
    }
}