using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerGuidSchema : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema is OpenApiSchema concreteSchema)
        {
            if (concreteSchema.Type == JsonSchemaType.String &&
                string.Equals(concreteSchema.Format, "uuid", StringComparison.OrdinalIgnoreCase))
            {
                concreteSchema.MaxLength = 36;
            }
        }

        return Task.CompletedTask;
    }
}
