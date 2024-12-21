using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerComponentSchemas() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.Schemas.Add("GenericString", new OpenApiSchema
        {
            Type = "string",
            Pattern = GlobalConfiguration.ApiSettings!.GenericBoundaries.Regex,
            MaxLength = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum
        });

        document.Components.Schemas.Add("GenericInt", new OpenApiSchema
        {
            Type = "integer",
            Format = "int32",
            Minimum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Minimum,
            Maximum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum,
        });

        return Task.CompletedTask;
    }
}