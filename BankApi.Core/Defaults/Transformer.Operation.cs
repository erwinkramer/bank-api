using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

class TransformerOperation(IAuthorizationPolicyProvider authorizationPolicyProvider) : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Responses ??= new();
        operation.Security ??= new List<OpenApiSecurityRequirement>();

        AddStandardResponses(operation, context.Document!);
        AddHeadersToResponses(operation, context.Document!);
        await AddSecurityPolicyToRequest(operation, authorizationPolicyProvider, context);
        AddMaxLengthToGuidParameters(operation);
    }

    private void AddStandardResponses(OpenApiOperation operation, OpenApiDocument document)
    {
        operation.Responses!["500"] = new OpenApiResponseReference("500", document);
        operation.Responses!["422"] = new OpenApiResponseReference("422", document);
        operation.Responses!["400"] = new OpenApiResponseReference("400", document);
        operation.Responses!["401"] = new OpenApiResponseReference("401", document);
        operation.Responses!["429"] = new OpenApiResponseReference("429", document);
    }

    private void AddHeadersToResponses(OpenApiOperation operation, OpenApiDocument document)
    {
        if (operation.Responses == null) return;

        foreach (var response in operation.Responses)
        {
            if (response.Value is OpenApiResponse concrete)
            {
                concrete.Headers ??= new Dictionary<string, IOpenApiHeader>();
                concrete.Headers["API-Version"] = new OpenApiHeaderReference("API-Version", document);
                concrete.Headers["Access-Control-Allow-Origin"] = new OpenApiHeaderReference("Access-Control-Allow-Origin", document);
                concrete.Headers["Access-Control-Expose-Headers"] = new OpenApiHeaderReference("GenericStringHeader", document);
                concrete.Headers["X-JWS-Signature"] = new OpenApiHeaderReference("GenericStringHeader", document);

                if (response.Key[0] is '2' or '4')
                {
                    concrete.Headers["X-Rate-Limit-Limit"] = new OpenApiHeaderReference("X-Rate-Limit-Limit", document);
                }
            }
        }
    }

    private async Task AddSecurityPolicyToRequest(OpenApiOperation operation, IAuthorizationPolicyProvider authorizationPolicyProvider, OpenApiOperationTransformerContext context)
    {
        var policyName = context.Description.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .FirstOrDefault();
        if (policyName == null) return;

        var policy = await authorizationPolicyProvider.GetPolicyAsync(policyName);
        if (policy == null) return;

        OpenApiSecurityRequirement securityRequirement = [];
        foreach (var policyScheme in policy.AuthenticationSchemes)
        {
            securityRequirement.Add(new OpenApiSecuritySchemeReference(policyScheme, context.Document), []);
            if (policyScheme == JwtBearerDefaults.AuthenticationScheme)
                securityRequirement.Add(new OpenApiSecuritySchemeReference("OpenIdConnect", context.Document), []);
        }
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(securityRequirement);
    }

    private void AddMaxLengthToGuidParameters(OpenApiOperation operation)
    {
        if (operation.Parameters == null) return;

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Schema is OpenApiSchema schema &&
                schema.Type == JsonSchemaType.String &&
                string.Equals(schema.Format, "uuid", StringComparison.OrdinalIgnoreCase))
            {
                schema.MaxLength = 36;
            }
        }
    }

}
