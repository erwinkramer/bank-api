using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

class TransformerDocInfo() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var server in GlobalConfiguration.ApiDocument!.Servers)
        {
            server.Extensions["x-internal"] = new OpenApiBoolean(false);
        }

        document.Info = GlobalConfiguration.ApiDocument!.Info;
        document.Servers = GlobalConfiguration.ApiDocument!.Servers;
        document.Components ??= new OpenApiComponents();

        document.Components.Schemas.Add("GenericString", new OpenApiSchema
        {
            Type = "string",
            Pattern = GlobalConfiguration.ApiSettings!.GenericBoundaries.Regex,
            MaxLength = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum
        });

        document.Components.Schemas.Add("GenericInt", new OpenApiSchema
        {
            Type = "integer",
            Format = "int32",
            Minimum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Minimum,
            Maximum = GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum,
        });

        document.Components.Headers.Add("GenericStringHeader", new OpenApiHeader
        {
            Schema = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GenericString" } }
        });

        //because of a bug in the spectral OWASP linter, we add the Access-Control-Allow-Origin header to the components and use it
        document.Components.Headers.Add("Access-Control-Allow-Origin", new OpenApiHeader
        {
            Schema = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GenericString" } }
        });

        document.Components.Headers.Add("X-RateLimit-Limit", CreateIntHeader($"The maximum number of requests you're permitted to make in a window of {GlobalConfiguration.ApiSettings!.FixedWindowRateLimit.Window.Minutes} minutes."));

        document.Components.Responses.Add("500", new OpenApiResponse
        {
            Description = "Internal server error.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "InternalServerError", new OpenApiMediaType { Schema = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GenericString" } } } }
            }
        });

        document.Components.Responses.Add("401", new OpenApiResponse
        {
            Description = "Unauthorized request.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "UnauthorizedRequest", new OpenApiMediaType { Schema = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GenericString" } } } }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                { "WWW-Authenticate", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "GenericStringHeader" } } }
            }
        });

        document.Components.Responses.Add("429", new OpenApiResponse
        {
            Description = "Too many requests.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                { "TooManyRequests", new OpenApiMediaType { Schema = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GenericString" } } } }
            },
            Headers = new Dictionary<string, OpenApiHeader>
            {
                { "Retry-After", CreateIntHeader("The number of seconds to wait before retrying the request.") }
            }
        });

        AddHeadersToResponses(document.Components);

        return Task.CompletedTask;
    }

    private void AddHeadersToResponses(OpenApiComponents components)
    {
        foreach (var response in components.Responses)
        {
            response.Value.Headers.Add("Access-Control-Allow-Origin", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "Access-Control-Allow-Origin" } });
            response.Value.Headers.Add("Access-Control-Expose-Headers", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "GenericStringHeader" } });

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers.Add("X-RateLimit-Limit", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "X-RateLimit-Limit" } });
            }
        }
    }

    private OpenApiHeader CreateIntHeader(string? description) => new OpenApiHeader
    {
        Description = description,
        Schema = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GenericInt" } }
    };
}