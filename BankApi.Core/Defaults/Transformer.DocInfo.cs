using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerDocInfo() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var server in GlobalConfiguration.ApiDocument!.Servers)
        {
            server.Extensions["x-internal"] = null; // https://github.com/microsoft/OpenAPI.NET/issues/2151
        }

        document.Info = GlobalConfiguration.ApiDocument!.Info;
        document.Servers = GlobalConfiguration.ApiDocument!.Servers;

        return Task.CompletedTask;
    }
}