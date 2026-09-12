using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerExampleSchema : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (GlobalConfiguration.ApiExamples is not JsonObject apiExamples)
            return;

        // Every example key equals the type name, except Paging<BankModel>.
        string? key = context.JsonTypeInfo.Type switch
        {
            Type t when t == typeof(Paging<BankModel>) => "PagingOfBankModel",
            Type t when t == typeof(BankModel) || t == typeof(BankEvent)
                || t == typeof(TellerReportList) || t == typeof(Teller) => t.Name,
            _ => null
        };

        if (key is null)
            return;

        if (apiExamples[key] is JsonArray examples)
            schema.Examples = [.. examples.OfType<JsonNode>()];
    }
}
