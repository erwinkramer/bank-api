using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerOperation : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        AddStandardResponses(operation);
        AddHeadersToResponses(operation);

        return Task.CompletedTask;
    }

    private void AddStandardResponses(OpenApiOperation operation)
    {
        operation.Responses.Add("500", new OpenApiResponse
        {
            Reference = new OpenApiReference { Type = ReferenceType.Response, Id = "500" }
        });

        operation.Responses.Add("401", new OpenApiResponse
        {
            Reference = new OpenApiReference { Type = ReferenceType.Response, Id = "401" }
        });

        operation.Responses.Add("429", new OpenApiResponse
        {
            Reference = new OpenApiReference { Type = ReferenceType.Response, Id = "429" }
        });
    }

    private void AddHeadersToResponses(OpenApiOperation operation)
    {
        foreach (var response in operation.Responses)
        {
            response.Value.Headers.Add("Access-Control-Allow-Origin", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "Access-Control-Allow-Origin" } });
            response.Value.Headers.Add("Access-Control-Expose-Headers", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "GenericStringHeader" } });

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers.Add("X-RateLimit-Limit", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "X-RateLimit-Limit" } });
            }
        }
    }
}
