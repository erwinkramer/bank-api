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
        await AddSecurityToRequest(operation, authorizationPolicyProvider, context);
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

    private async Task AddSecurityToRequest(OpenApiOperation operation, IAuthorizationPolicyProvider authorizationPolicyProvider, OpenApiOperationTransformerContext actionDescriptor)
    {
        var securityRequirement = new OpenApiSecurityRequirement();

        var policy = actionDescriptor.Description.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .FirstOrDefault();
        if (policy == null) return;

        var endpointPolicy = await authorizationPolicyProvider.GetPolicyAsync(policy);
        if (endpointPolicy == null) return;

        foreach (var scheme in endpointPolicy.AuthenticationSchemes)
        {
            securityRequirement.Add(OpenApiFactory.CreateSecuritySchemaRef(scheme), new List<string>());
            if (scheme == JwtBearerDefaults.AuthenticationScheme)
                securityRequirement.Add(OpenApiFactory.CreateSecuritySchemaRef("OpenIdConnect"), new List<string>());
        }
        operation.Security.Add(securityRequirement);
    }
}
