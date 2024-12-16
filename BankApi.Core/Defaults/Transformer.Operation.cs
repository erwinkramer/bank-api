using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerOperation() : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Responses.Add("500", new OpenApiResponse { 
            Description = "Internal server error." ,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    "InternalServerError", new OpenApiMediaType { Schema =
                        new OpenApiSchema { Type = "string" ,  Pattern = ".*",  MaxLength = 7070 }
                    }
                }
            },
        });

        operation.Responses.Add("401", new OpenApiResponse
        {
            Description = "Unauthorized request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    "UnauthorizedRequest", new OpenApiMediaType { Schema =
                        new OpenApiSchema { Type = "string" ,  Pattern = ".*",  MaxLength = 7070 }
                    }
                }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                {
                    "WWW-Authenticate", new OpenApiHeader { Schema =
                        new OpenApiSchema {  Type = "string", Pattern = ".*", MaxLength = 7070 }
                    }
                }
            }
        });

        operation.Responses.Add("429", new OpenApiResponse
        {
            Description = "Too many requests.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    "TooManyRequests", new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string", Pattern = ".*", MaxLength = 7070  } }
                }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                {
                    "Retry-After", new OpenApiHeader { Schema =
                        new OpenApiSchema { Type = "integer", Format = "int32", Minimum = 0, Maximum = 70707070 }
                    }
                }
            }
        });

        foreach (var response in operation.Responses)
        {
            response.Value.Headers.Add(
                "Access-Control-Allow-Origin", new OpenApiHeader
                {
                    Schema = new OpenApiSchema { Type = "string", Pattern = ".*", MaxLength = 7070 }
                }
            );

            if (response.Key.StartsWith("2") || response.Key.StartsWith("4"))
            {
                response.Value.Headers.Add(
                    "X-RateLimit-Limit", new OpenApiHeader
                    {
                        Schema = new OpenApiSchema { Type = "integer", Format = "int32", Minimum = 0, Maximum = 70707070, Description = "The maximum number of requests you're permitted to make per hour." }
                    }
                );
            }
        }

        return Task.CompletedTask;
    }
}