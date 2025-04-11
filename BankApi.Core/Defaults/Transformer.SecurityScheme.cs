using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;

class TransformerSecurityScheme(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        foreach (AuthenticationScheme authenticationScheme in authenticationSchemes)
        {
            OpenApiSecurityScheme[] securitySchemes = authenticationScheme.Name switch
            {
                JwtBearerDefaults.AuthenticationScheme =>
                [
                    new OpenApiSecurityScheme
                    {
                        Description = "Bearer scheme, please see: https://learn.openapis.org/specification/security.html#http-authentication.",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token"
                    },
                    new () // for Scalar UI or any other API Management UI
                    {
                        Description = "OpenID Connect scheme, please see: https://learn.openapis.org/specification/security.html#openid-connect.",
                        Type = SecuritySchemeType.OpenIdConnect,
                        OpenIdConnectUrl = new ($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/v2.0/.well-known/openid-configuration")
                    }
                ],
                $"{ApiKeyDefaults.AuthenticationScheme}-Header" =>
                [
                    new ()
                    {
                        Type = SecuritySchemeType.ApiKey,
                        Description = "Api Key scheme, please see: https://learn.openapis.org/specification/security.html#api-keys.",
                        Name = "Ocp-Apim-Subscription-Key",
                        In = ParameterLocation.Header
                    }
                ],
                _ => []
            };

            foreach (var scheme in securitySchemes)
            {
                document.Components.SecuritySchemes[$"{scheme.Name?? ""}{scheme.Type}"] = scheme;
                document.SecurityRequirements.Add(new() { { new OpenApiSecuritySchemeReference($"{scheme.Name?? ""}{scheme.Type}"), new List<string>() } });
            }
        }
    }
}
