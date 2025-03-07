using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;

class TransformerComponentResponses() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new();
        document.Components.Responses ??= new Dictionary<string, OpenApiResponse>();

        document.Components.Responses["500"] = new()
        {
            Description = "Internal server error.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "application/problem+json", new () { Schema = new OpenApiSchemaReference("Problem", document) } }
            }
        };

        document.Components.Responses["400"] = new()
        {
            Description = "Bad request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "application/problem+json", new () { Schema = new OpenApiSchemaReference("Problem", document) } }
            }
        };

        document.Components.Responses["422"] = new()
        {
            Description = "Unprocessable Entity.",
            Content = new Dictionary<string, OpenApiMediaType>()
            {
                { "application/problem+json", new () { Schema = new OpenApiSchemaReference("Problem", document) } }
            }
        };

        document.Components.Responses["401"] = new()
        {
            Description = "Unauthorized request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "NoContent" , new () }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                { "WWW-Authenticate", new OpenApiHeaderReference("GenericStringHeader", document) }
            }
        };

        document.Components.Responses["429"] = new()
        {
            Description = "Too many requests.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "TooManyRequests", new () { Schema = new OpenApiSchemaReference("GenericString", document) } }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                { "Retry-After", OpenApiFactory.CreateHeaderInt(document, "The number of seconds to wait before retrying the request.") }
            }
        };

        AddHeadersToResponses(document);

        return Task.CompletedTask;
    }

    private void AddHeadersToResponses(OpenApiDocument doc)
    {
        foreach (var response in doc.Components.Responses)
        {
            response.Value.Headers["Access-Control-Allow-Origin"] = new OpenApiHeaderReference("Access-Control-Allow-Origin", doc);
            response.Value.Headers["Access-Control-Expose-Headers"] = new OpenApiHeaderReference("GenericStringHeader", doc);

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers["X-RateLimit-Limit"] = new OpenApiHeaderReference("X-RateLimit-Limit", doc);
            }
        }
    }
}