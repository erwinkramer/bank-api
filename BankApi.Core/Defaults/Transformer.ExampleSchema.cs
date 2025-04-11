using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerExampleSchema : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (GlobalConfiguration.ApiExamples is not JsonObject apiExamples)
            return Task.CompletedTask;

        schema.Example = context.JsonTypeInfo.Type switch
        {
            Type t when t == typeof(BankModel) =>
                (apiExamples["BankModel"] as JsonArray)?[0],

            Type t when t == typeof(Paging<BankModel>) =>
                (apiExamples["PagingOfBankModel"] as JsonArray)?[0],

            Type t when t == typeof(TellerReportList) =>
                (apiExamples["TellerReportList"] as JsonArray)?[0],

            Type t when t == typeof(Teller) =>
                (apiExamples["Teller"] as JsonArray)?[0],

            _ => schema.Example
        };

        return Task.CompletedTask;
    }
}
