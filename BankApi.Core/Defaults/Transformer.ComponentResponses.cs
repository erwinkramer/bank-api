using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerComponentResponses() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new();
        document.Components.Responses ??= new Dictionary<string, IOpenApiResponse>();

        document.Components.Responses["500"] = new OpenApiResponse
        {
            Description = "Internal server error.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "application/problem+json", new () { Schema = new OpenApiSchemaReference("Problem", document) } }
            },
            Headers = new Dictionary<string, IOpenApiHeader>()
        };

        document.Components.Responses["400"] = new OpenApiResponse
        {
            Description = "Bad request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "application/problem+json", new () { Schema = new OpenApiSchemaReference("Problem", document) } }
            },
            Headers = new Dictionary<string, IOpenApiHeader>()
        };

        document.Components.Responses["422"] = new OpenApiResponse
        {
            Description = "Unprocessable Entity.",
            Content = new Dictionary<string, OpenApiMediaType>()
            {
                { "application/problem+json", new () { Schema = new OpenApiSchemaReference("Problem", document) } }
            },
            Headers = new Dictionary<string, IOpenApiHeader>()
        };

        document.Components.Responses["401"] = new OpenApiResponse
        {
            Description = "Unauthorized request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "NoContent" , new () }
            },
            Headers = new Dictionary<string, IOpenApiHeader>
            {
                { "WWW-Authenticate", new OpenApiHeaderReference("GenericStringHeader", document) }
            }
        };

        document.Components.Responses["429"] = new OpenApiResponse
        {
            Description = "Too many requests.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "TooManyRequests", new () { Schema = new OpenApiSchemaReference("GenericString", document) } }
            },
            Headers = new Dictionary<string, IOpenApiHeader>
            {
                { "Retry-After", OpenApiFactory.CreateHeaderInt(document, "The number of seconds to wait before retrying the request.") }
            }
        };

        AddHeadersToResponses(document);

        return Task.CompletedTask;
    }

    private void AddHeadersToResponses(OpenApiDocument doc)
    {
        if (doc.Components?.Responses == null) return;

        foreach (var response in doc.Components.Responses)
        {
            response.Value.Headers!["API-Version"] = new OpenApiHeaderReference("API-Version", doc);
            response.Value.Headers["Access-Control-Allow-Origin"] = new OpenApiHeaderReference("Access-Control-Allow-Origin", doc);
            response.Value.Headers["Access-Control-Expose-Headers"] = new OpenApiHeaderReference("GenericStringHeader", doc);
            response.Value.Headers["X-JWS-Signature"] = new OpenApiHeaderReference("GenericStringHeader", doc);

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers["X-Rate-Limit-Limit"] = new OpenApiHeaderReference("X-Rate-Limit-Limit", doc);
            }
        }
    }
}