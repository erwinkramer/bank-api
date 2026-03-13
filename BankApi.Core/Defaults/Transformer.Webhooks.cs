using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

class TransformerWebhooks() : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Webhooks ??= new Dictionary<string, IOpenApiPathItem>();

        document.Webhooks["CreateBank"] = new OpenApiPathItem
        {
            Operations = new()
            {
                [HttpMethod.Post] = new OpenApiOperation
                {
                    OperationId = "CreateBankWebhook",
                    Summary = "Create bank event",
                    Description = "Notifies consumers when a bank is created.",
                    RequestBody = new OpenApiRequestBody
                    {
                        Required = true,
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/cloudevents+json; charset=utf-8"] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchemaReference("BankEvent", document)
                            }
                        }
                    },
                    Responses = new OpenApiResponses
                    {
                        ["201"] = new OpenApiResponseReference("201", document),
                        ["410"] = new OpenApiResponseReference("410", document),
                        ["415"] = new OpenApiResponseReference("415", document),
                        ["429"] = new OpenApiResponseReference("429", document)
                    }
                }
            }
        };
    }
}