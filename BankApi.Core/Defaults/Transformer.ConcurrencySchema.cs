using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerConcurrencySchema : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var property in context.JsonTypeInfo.Properties.Where(property => property.AttributeProvider?.IsDefined(typeof(ConcurrencyCheckAttribute), true) == true))
        {
            schema.Properties?.Remove(property.Name);
            schema.Required?.Remove(property.Name);
        }
    }
}