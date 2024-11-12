using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerOperation() : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Responses.Add("500", new OpenApiResponse { Description = "Internal server error" });

        return Task.CompletedTask;
    }
}