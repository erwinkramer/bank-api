using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models.References;

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
        operation.Responses["500"] = new OpenApiResponseReference("500");
        operation.Responses["422"] = new OpenApiResponseReference("422");
        operation.Responses["400"] = new OpenApiResponseReference("400");
        operation.Responses["401"] = new OpenApiResponseReference("401");
        operation.Responses["429"] = new OpenApiResponseReference("429");
    }

    private void AddHeadersToResponses(OpenApiOperation operation)
    {
        foreach (var response in operation.Responses)
        {
            response.Value.Headers["Access-Control-Allow-Origin"] = new OpenApiHeaderReference("Access-Control-Allow-Origin"); 
            response.Value.Headers["Access-Control-Expose-Headers"] = new OpenApiHeaderReference("GenericStringHeader");

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers["X-RateLimit-Limit"] = new OpenApiHeaderReference("X-RateLimit-Limit");
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
            securityRequirement.Add(new OpenApiSecuritySchemeReference(policyScheme), []);
            if (policyScheme == JwtBearerDefaults.AuthenticationScheme)
                securityRequirement.Add(new OpenApiSecuritySchemeReference("OpenIdConnect"), []);
        }
        operation.Security.Add(securityRequirement);
    }
}
