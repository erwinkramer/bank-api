using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

class TransformerOperation(IAuthorizationPolicyProvider authorizationPolicyProvider) : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        AddStandardResponses(operation);
        AddHeadersToResponses(operation);
        await AddSecurityPolicyToRequest(operation, authorizationPolicyProvider, context);
    }

    private void AddStandardResponses(OpenApiOperation operation)
    {
        operation.Responses["500"] = new() { Reference = new() { Type = ReferenceType.Response, Id = "500" } };
        operation.Responses["422"] = new() { Reference = new() { Type = ReferenceType.Response, Id = "422" } };
        operation.Responses["400"] = new() { Reference = new() { Type = ReferenceType.Response, Id = "400" } };
        operation.Responses["401"] = new() { Reference = new() { Type = ReferenceType.Response, Id = "401" } };
        operation.Responses["429"] = new() { Reference = new() { Type = ReferenceType.Response, Id = "429" } };
    }

    private void AddHeadersToResponses(OpenApiOperation operation)
    {
        foreach (var response in operation.Responses)
        {
            response.Value.Headers["Access-Control-Allow-Origin"] = new() { Reference = new() { Type = ReferenceType.Header, Id = "Access-Control-Allow-Origin" } };
            response.Value.Headers["Access-Control-Expose-Headers"] = new() { Reference = new() { Type = ReferenceType.Header, Id = "GenericStringHeader" } };

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers["X-RateLimit-Limit"] = new() { Reference = new() { Type = ReferenceType.Header, Id = "X-RateLimit-Limit" } };
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
            securityRequirement.Add(OpenApiFactory.CreateSecuritySchemaRef(policyScheme), []);
            if (policyScheme == JwtBearerDefaults.AuthenticationScheme)
                securityRequirement.Add(OpenApiFactory.CreateSecuritySchemaRef("OpenIdConnect"), []);
        }
        operation.Security.Add(securityRequirement);
    }
}
