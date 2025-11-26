using System.Text.Json.Nodes;
using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

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
                    },
                    {
                        "OAuth2",
                        new OpenApiSecurityScheme
                        {
                            Description = "OAuth2 scheme, supports RFC8725. Please see: https://learn.openapis.org/specification/security.html#oauth2-1.",
                            Type = SecuritySchemeType.OAuth2,
                            Flows = new OpenApiOAuthFlows
                            {
                                AuthorizationCode = new OpenApiOAuthFlow
                                {
                                    AuthorizationUrl = new ($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/authorize"),
                                    TokenUrl = new ($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/token"),
                                    RefreshUrl = new ($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/token"),
                                    Scopes = new Dictionary<string, string>
                                    {
                                        { $"{GlobalConfiguration.ApiSettings!.EntraId.ClientId}/.default", "Access to Bank API" }
                                    },
                                    Extensions = new Dictionary<string, IOpenApiExtension>
                                    {
                                        { "x-client-id",  new JsonNodeExtension(JsonValue.Create( GlobalConfiguration.ApiSettings!.EntraId.ClientId)) }
                                    }
                                },
                                ClientCredentials = new OpenApiOAuthFlow{
                                    TokenUrl = new ($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/token"),
                                    RefreshUrl = new ($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/oauth2/v2.0/token"),
                                    Scopes = new Dictionary<string, string>
                                    {
                                        { $"{GlobalConfiguration.ApiSettings!.EntraId.ClientId}/.default", "Access to Bank API" }
                                    },
                                    Extensions = new Dictionary<string, IOpenApiExtension>
                                    {
                                        { "x-client-id",  new JsonNodeExtension(JsonValue.Create( GlobalConfiguration.ApiSettings!.EntraId.ClientId)) }
                                    }
                                }
                            }
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
