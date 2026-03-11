using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerGuidSchema : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Type == JsonSchemaType.String &&
            string.Equals(schema.Format, "uuid", StringComparison.OrdinalIgnoreCase))
        {
            schema.MaxLength = 36;
        }
    }
}
