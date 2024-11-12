using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

class TransformerExampleSchema() : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(BankModel))
        {
            schema.Example = (GlobalConfiguration.ApiExamples as OpenApiArray)![1];
        }

        return Task.CompletedTask;
    }
}
