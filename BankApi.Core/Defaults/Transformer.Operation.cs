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
        var responses = new[]
        {
            (
                statusCode: "500",
                description: "Internal server error.",
                contentKey: "InternalServerError",
                schema: CreateStringSchema(),
                headers:  new Dictionary<string, OpenApiHeader>()
            ),
            (
                statusCode: "401",
                description: "Unauthorized request.",
                contentKey: "UnauthorizedRequest",
                schema: CreateStringSchema(),
                headers: new Dictionary<string, OpenApiHeader>
                {
                    { "WWW-Authenticate", CreateStringHeader() }
                }
            ),
            (
                statusCode: "429",
                description: "Too many requests.",
                contentKey: "TooManyRequests",
                schema: CreateStringSchema(),
                headers: new Dictionary<string, OpenApiHeader>
                {
                    { "Retry-After", CreateIntHeader(null) }
                }
            )
        };

        foreach (var (statusCode, description, contentKey, schema, headers) in responses)
        {
            operation.Responses.Add(statusCode, new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    { contentKey, new OpenApiMediaType { Schema = schema } }
                },
                Headers = headers
            });
        }
    }

    private void AddHeadersToResponses(OpenApiOperation operation)
    {
        foreach (var response in operation.Responses)
        {
            response.Value.Headers.Add("Access-Control-Allow-Origin", CreateStringHeader());
            response.Value.Headers.Add("Access-Control-Expose-Headers", CreateStringHeader());

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers.Add("X-RateLimit-Limit", CreateIntHeader("The maximum number of requests you're permitted to make per hour."));
            }
        }
    }

    private OpenApiSchema CreateStringSchema() => new OpenApiSchema
    {
        Type = "string",
        Pattern = ".*",
        MaxLength = Int32.MaxValue
    };

    private OpenApiHeader CreateStringHeader() => new OpenApiHeader
    {
        Schema = CreateStringSchema()
    };

    private OpenApiHeader CreateIntHeader(string? description) => new OpenApiHeader
    {
        Schema = new OpenApiSchema
        {
            Type = "integer",
            Format = "int32",
            Minimum = 0,
            Maximum = Int32.MaxValue,
            Description = description
        }
    };
}
