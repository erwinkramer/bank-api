using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;

class TransformerComponentSchemas() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new();
        document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

        document.Components.Schemas["GenericString"] = new()
        {
            Type = JsonSchemaType.String,
            Pattern = GlobalConfiguration.ApiSettings!.GenericBoundaries.Regex,
            MaxLength = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum
        };

        document.Components.Schemas["GenericInt"] = new()
        {
            Type = JsonSchemaType.Integer,
            Format = "int32",
            Minimum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Minimum,
            Maximum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum,
        };

        document.Components.Schemas["Problem"] = new()
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchemaReference("GenericString", document),
                ["title"] = new OpenApiSchemaReference("GenericString", document),
                ["status"] =  new OpenApiSchemaReference("GenericInt", document),
                ["detail"] =  new OpenApiSchemaReference("GenericString", document),
                ["instance"] =  new OpenApiSchemaReference("GenericString", document),
                ["traceId"] =  new OpenApiSchemaReference("GenericString", document),
                ["requestId"] =  new OpenApiSchemaReference("GenericString", document)
            }
        };

        return Task.CompletedTask;
    }
}