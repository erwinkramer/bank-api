using Gridify;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

class TransformerExampleSchema : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var apiExamples = GlobalConfiguration.ApiExamples as OpenApiObject;
        if (apiExamples == null)
            return Task.CompletedTask;

        schema.Example = context.JsonTypeInfo.Type switch
        {
            Type t when t == typeof(BankModel) =>
                (apiExamples["BankModel"] as OpenApiArray)?[0],

            Type t when t == typeof(AnnotatedPaging<BankModel>) =>
                (apiExamples["AnnotatedPagingOfBankModel"] as OpenApiArray)?[0],

            Type t when t == typeof(TellerReportList) =>
                (apiExamples["TellerReportList"] as OpenApiArray)?[0],

            Type t when t == typeof(Teller) =>
                (apiExamples["Teller"] as OpenApiArray)?[0],

            _ => schema.Example
        };

        return Task.CompletedTask;
    }
}
