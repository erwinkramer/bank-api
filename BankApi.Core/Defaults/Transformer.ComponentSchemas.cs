using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerComponentSchemas() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new ();
        document.Components.Schemas["GenericString"] = new ()
        {
            Type = "string",
            Pattern = GlobalConfiguration.ApiSettings!.GenericBoundaries.Regex,
            MaxLength = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum
        };

        document.Components.Schemas["GenericInt"] = new ()
        {
            Type = "integer",
            Format = "int32",
            Minimum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Minimum,
            Maximum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum,
        };

        document.Components.Schemas["Problem"] = new ()
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = OpenApiFactory.CreateSchemaRef("GenericString"),
                ["title"] = OpenApiFactory.CreateSchemaRef("GenericString"),
                ["status"] = OpenApiFactory.CreateSchemaRef("GenericInt"),
                ["detail"] = OpenApiFactory.CreateSchemaRef("GenericString"),
                ["instance"] = OpenApiFactory.CreateSchemaRef("GenericString"),
                ["traceId"] = OpenApiFactory.CreateSchemaRef("GenericString"),
                ["requestId"] = OpenApiFactory.CreateSchemaRef("GenericString")
            }
        };

        return Task.CompletedTask;
    }
}