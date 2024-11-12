static partial class ApiBuilder
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(GlobalConfiguration.ApiDocument!.Info.Version, options =>
        {
            options.AddDocumentTransformer<TransformerDocInfo>();
            options.AddDocumentTransformer<TransformerSecurityScheme>();
            options.AddSchemaTransformer<TransformerExampleSchema>();
            options.AddOperationTransformer<TransformerOperation>();
        });

        return services;
    }
}