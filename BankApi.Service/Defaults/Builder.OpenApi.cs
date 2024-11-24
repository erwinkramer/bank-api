using Scalar.AspNetCore;

static partial class ApiBuilder
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        services.AddOpenApi(GlobalConfiguration.ApiDocument!.Info.Version, options =>
        {
            options.AddDocumentTransformer<TransformerDocInfo>();
            options.AddDocumentTransformer<TransformerSecurityScheme>();
            options.AddSchemaTransformer<TransformerExampleSchema>();
            options.AddOperationTransformer<TransformerOperation>();
        });

        return services;
    }

    public static void AddOpenApiScalarReference(this IEndpointRouteBuilder app)
    {
        app.MapScalarApiReference(options =>
        {
            options.Theme = ScalarTheme.DeepSpace;
            options.WithApiKeyAuthentication(options =>
            {
                options.Token = "Lifetime Subscription";
            });
            options.Title = $"{GlobalConfiguration.ApiDocument!.Info.Title} docs | {GlobalConfiguration.ApiDocument.Info.Version}";
        });
    }
}