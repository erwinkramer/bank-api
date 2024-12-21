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
            Description = "Internal server error.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "InternalServerError", new OpenApiMediaType { Schema = CreateStringSchema() } }
            }
        });

        operation.Responses.Add("401", new OpenApiResponse
        {
            Description = "Unauthorized request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "UnauthorizedRequest", new OpenApiMediaType { Schema = CreateStringSchema() } }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                { "WWW-Authenticate", CreateStringHeader() }
            }
        });

        operation.Responses.Add("429", new OpenApiResponse
        {
            Description = "Too many requests.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "TooManyRequests", new OpenApiMediaType { Schema = CreateStringSchema() } }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                { "Retry-After", CreateIntHeader(null) }
            }
        });
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
        Pattern = GlobalConfiguration.ApiSettings!.GenericBoundaries.Regex,
        MaxLength = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum
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
            Minimum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Minimum,
            Maximum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum,
            Description = description
        }
    };
}
