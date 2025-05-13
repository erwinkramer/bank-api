using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models.References;

class TransformerOperation(IAuthorizationPolicyProvider authorizationPolicyProvider) : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Responses ??= new();
        operation.Security ??= new List<OpenApiSecurityRequirement>();

        AddStandardResponses(operation, context.Document!);
        AddHeadersToResponses(operation, context.Document!);
        await AddSecurityPolicyToRequest(operation, authorizationPolicyProvider, context);
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
            if(response.Value.Headers == null)
                continue; //TODO https://github.com/dotnet/aspnetcore/issues/61898

            response.Value.Headers["API-Version"] = new OpenApiHeaderReference("API-Version", document);
            response.Value.Headers["Access-Control-Allow-Origin"] = new OpenApiHeaderReference("Access-Control-Allow-Origin", document);
            response.Value.Headers["Access-Control-Expose-Headers"] = new OpenApiHeaderReference("GenericStringHeader", document);

            if (response.Key[0] is '2' or '4')
            {
                response.Value.Headers["X-Rate-Limit-Limit"] = new OpenApiHeaderReference("X-Rate-Limit-Limit", document);
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
}
