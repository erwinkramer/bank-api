using DownstreamClients.GitHub.Models;
using Gridify;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

class TransformerExampleSchema() : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var apiExamples = GlobalConfiguration.ApiExamples as OpenApiObject; ;

        if (context.JsonTypeInfo.Type == typeof(BankModel))
        {
            schema.Example = (apiExamples!["BankModel"] as OpenApiArray)![0];
        }

        if (context.JsonTypeInfo.Type == typeof(Paging<BankModel>))
        {
            schema.Example = (apiExamples!["PagingOfBankModel"] as OpenApiArray)![0];
        }

        if (context.JsonTypeInfo.Type == typeof(List<TellerReport>))
        {
            schema.Example = (apiExamples!["ListTellerReport"] as OpenApiArray)![0];
        }

        if (context.JsonTypeInfo.Type == typeof(Release))
        {
            schema.Example = (apiExamples!["Release"] as OpenApiArray)![0];
        }

        return Task.CompletedTask;
    }
}
