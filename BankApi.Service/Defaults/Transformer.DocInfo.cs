using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerDocInfo() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = GlobalConfiguration.ApiDocument!.Info;
        document.Servers = GlobalConfiguration.ApiDocument!.Servers;

        return Task.CompletedTask;
    }
}