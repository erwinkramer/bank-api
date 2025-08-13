using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerGuidSchema : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Type == JsonSchemaType.String &&
            string.Equals(schema.Format, "uuid", StringComparison.OrdinalIgnoreCase))
        {
            schema.MaxLength = 36;
        }

        return Task.CompletedTask;
    }
}
