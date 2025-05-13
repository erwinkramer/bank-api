using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;

class TransformerSecurityScheme(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        foreach (AuthenticationScheme authenticationScheme in authenticationSchemes)
        {
            Dictionary<string, OpenApiSecurityScheme> securitySchemes = authenticationScheme.Name switch
            {
                JwtBearerDefaults.AuthenticationScheme => new()
                {
                    {
                        JwtBearerDefaults.AuthenticationScheme,
                        new OpenApiSecurityScheme
                        {
                            Description = "Bearer scheme, please see: https://learn.openapis.org/specification/security.html#http-authentication.",
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            In = ParameterLocation.Header,
                            BearerFormat = "Json Web Token"
                        }
                    },
                    {
                        "OpenIdConnect",
                        new OpenApiSecurityScheme
                        {
                            Description = "OpenID Connect scheme, please see: https://learn.openapis.org/specification/security.html#openid-connect.",
                            Type = SecuritySchemeType.OpenIdConnect,
                            OpenIdConnectUrl = new ($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/v2.0/.well-known/openid-configuration")
                        }
                    }
                },
                $"{ApiKeyDefaults.AuthenticationScheme}-Header" => new()
                {
                    {
                        $"{ApiKeyDefaults.AuthenticationScheme}-Header",
                        new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.ApiKey,
                            Description = "Api Key scheme, please see: https://learn.openapis.org/specification/security.html#api-keys.",
                            Name = "Ocp-Apim-Subscription-Key",
                            In = ParameterLocation.Header
                        }
                    }
                },
                _ => new()
            };

            foreach (var scheme in securitySchemes)
            {
                if (scheme.Key is null) continue;

                document.Components.SecuritySchemes[scheme.Key] = scheme.Value;
                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(new()
                {
                    { new OpenApiSecuritySchemeReference(scheme.Key, document), [] }
                });
            }
        }
    }
}
