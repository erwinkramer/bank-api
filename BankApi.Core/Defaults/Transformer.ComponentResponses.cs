using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerComponentResponses() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.Responses["500"] = new OpenApiResponse
        {
            Description = "Internal server error.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "application/problem+json", new OpenApiMediaType { Schema = OpenApiFactory.CreateSchemaRef("Problem") } }
            }
        };

        document.Components.Responses["400"] = new OpenApiResponse
        {
            Description = "Bad request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "application/problem+json", new OpenApiMediaType { Schema = OpenApiFactory.CreateSchemaRef("Problem") } }
            }
        };

        document.Components.Responses["422"] = new OpenApiResponse
        {
            Description = "Unprocessable Entity.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "application/problem+json", new OpenApiMediaType { Schema = OpenApiFactory.CreateSchemaRef("Problem") } }
            }
        };

        document.Components.Responses["401"] = new OpenApiResponse
        {
            Description = "Unauthorized request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "NoContent" , new OpenApiMediaType {  } }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                { "WWW-Authenticate", OpenApiFactory.CreateHeaderRef("GenericStringHeader") }
            }
        };

        document.Components.Responses["429"] = new OpenApiResponse
        {
            Description = "Too many requests.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "TooManyRequests", new OpenApiMediaType { Schema = OpenApiFactory.CreateSchemaRef("GenericString") } }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                { "Retry-After", OpenApiFactory.CreateHeaderInt("The number of seconds to wait before retrying the request.") }
            }
        };

        AddHeadersToResponses(document.Components);

        return Task.CompletedTask;
    }

    private void AddHeadersToResponses(OpenApiComponents components)
    {
        foreach (var response in components.Responses)
        {
            response.Value.Headers["Access-Control-Allow-Origin"] = OpenApiFactory.CreateHeaderRef("Access-Control-Allow-Origin");
            response.Value.Headers["Access-Control-Expose-Headers"] = OpenApiFactory.CreateHeaderRef("GenericStringHeader");

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers["X-RateLimit-Limit"] = OpenApiFactory.CreateHeaderRef("X-RateLimit-Limit");
            }
        }
    }
}