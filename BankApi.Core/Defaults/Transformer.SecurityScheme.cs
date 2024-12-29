using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

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
                    new ()
                    {
                        Description = "Bearer scheme, please see: https://learn.openapis.org/specification/security.html#http-authentication.",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token",
                        Reference = new ()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    new () // for Scalar UI or any other API Management UI
                    {
                        Description = "OpenID Connect scheme, please see: https://learn.openapis.org/specification/security.html#openid-connect.",
                        Type = SecuritySchemeType.OpenIdConnect,
                        OpenIdConnectUrl = new ($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/v2.0/.well-known/openid-configuration"),
                        Reference = new ()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "OpenIdConnect"
                        }
                    }
                ],
                $"{ApiKeyDefaults.AuthenticationScheme}-Header" =>
                [
                    new ()
                    {
                        Type = SecuritySchemeType.ApiKey,
                        Description = "Api Key scheme, please see: https://learn.openapis.org/specification/security.html#api-keys.",
                        Name = "Ocp-Apim-Subscription-Key",
                        In = ParameterLocation.Header,
                        Reference = new ()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = $"{ApiKeyDefaults.AuthenticationScheme}-Header"
                        }
                    }
                ],
                _ => []
            };

            foreach (var securityScheme in securitySchemes)
            {
                document.Components.SecuritySchemes[securityScheme.Reference.Id] = securityScheme;
                document.SecurityRequirements.Add(new () { { securityScheme, new List<string>() } });
            }
        }
    }
}
