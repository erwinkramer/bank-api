using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerComponentSchemas() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        document.Components.Schemas["GenericString"] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Pattern = GlobalConfiguration.ApiSettings!.GenericBoundaries.Regex,
            MaxLength = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum
        };

        document.Components.Schemas["GenericInt"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Integer,
            Format = "int32",
            Minimum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Minimum.ToString(),
            Maximum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum.ToString(),
        };

        document.Components.Schemas["Problem"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchemaReference("GenericString", document),
                ["title"] = new OpenApiSchemaReference("GenericString", document),
                ["status"] = new OpenApiSchemaReference("GenericInt", document),
                ["detail"] = new OpenApiSchemaReference("GenericString", document),
                ["instance"] = new OpenApiSchemaReference("GenericString", document),
                ["traceId"] = new OpenApiSchemaReference("GenericString", document),
                ["requestId"] = new OpenApiSchemaReference("GenericString", document),
                ["errors"] = new OpenApiSchema // matches https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpvalidationproblemdetails.errors?view=aspnetcore-10.0#microsoft-aspnetcore-http-httpvalidationproblemdetails-errors
                {
                    Type = JsonSchemaType.Object,
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchemaReference("GenericString", document),
                        MaxItems = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum,
                    }
                }
            }
        };

        return Task.CompletedTask;
    }
}