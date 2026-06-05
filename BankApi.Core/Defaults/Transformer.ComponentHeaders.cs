using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerComponentHeaders() : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new();
        document.Components.Headers ??= new Dictionary<string, IOpenApiHeader>();
        document.Components.Parameters ??= new Dictionary<string, IOpenApiParameter>();

        document.Components.Headers["GenericStringHeader"] = OpenApiFactory.CreateHeaderString(document);

        //because of a bug in the spectral OWASP linter, 
        //using the Access-Control-Allow-Origin header instead of `GenericStringHeader`
        //please see https://github.com/stoplightio/spectral-owasp-ruleset/issues/71
        document.Components.Headers["Access-Control-Allow-Origin"] = OpenApiFactory.CreateHeaderString(document);

        document.Components.Headers["API-Version"] = OpenApiFactory.CreateHeaderString(document);

        document.Components.Headers["ETag"] = OpenApiFactory.CreateHeaderString(document, "Current entity tag for optimistic concurrency. Send this value in the 'If-Match' header when doing update requests.");

        document.Components.Parameters["If-Match"] = new OpenApiParameter { Name = "If-Match", In = ParameterLocation.Header, Required = true, Description = "Current entity tag for optimistic concurrency.", Schema = new OpenApiSchemaReference("GenericString", document) };

        document.Components.Headers["X-Rate-Limit-Limit"] = OpenApiFactory.CreateHeaderInt(document, $"The maximum number of requests you're permitted to make in a window of {GlobalConfiguration.ApiSettings!.FixedWindowRateLimit.Window.Minutes} minutes.");
    }
}