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
        operation.Responses.Add("500", new OpenApiResponse
        {
            Reference = new OpenApiReference { Type = ReferenceType.Response, Id = "500" }
        });

        operation.Responses.Add("400", new OpenApiResponse
        {
            Reference = new OpenApiReference { Type = ReferenceType.Response, Id = "400" }
        });

        operation.Responses.Add("401", new OpenApiResponse
        {
            Reference = new OpenApiReference { Type = ReferenceType.Response, Id = "401" }
        });

        operation.Responses.Add("429", new OpenApiResponse
        {
            Reference = new OpenApiReference { Type = ReferenceType.Response, Id = "429" }
        });
    }

    private void AddHeadersToResponses(OpenApiOperation operation)
    {
        foreach (var response in operation.Responses)
        {
            response.Value.Headers.Add("Access-Control-Allow-Origin", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "Access-Control-Allow-Origin" } });
            response.Value.Headers.Add("Access-Control-Expose-Headers", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "GenericStringHeader" } });

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers.Add("X-RateLimit-Limit", new OpenApiHeader { Reference = new OpenApiReference { Type = ReferenceType.Header, Id = "X-RateLimit-Limit" } });
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

        var securityRequirement = new OpenApiSecurityRequirement();
        foreach (var policyScheme in policy.AuthenticationSchemes)
        {
            securityRequirement.Add(OpenApiFactory.CreateSecuritySchemaRef(policyScheme), new List<string>());
            if (policyScheme == JwtBearerDefaults.AuthenticationScheme)
                securityRequirement.Add(OpenApiFactory.CreateSecuritySchemaRef("OpenIdConnect"), new List<string>());
        }
        operation.Security.Add(securityRequirement);
    }
}
