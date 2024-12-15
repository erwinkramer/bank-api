using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

class TransformerSecurityScheme(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        foreach (AuthenticationScheme authenticationScheme in authenticationSchemes)
        {
            OpenApiSecurityScheme? securityScheme = authenticationScheme.Name switch
            {
                JwtBearerDefaults.AuthenticationScheme => new OpenApiSecurityScheme
                {
                    Description = "https://learn.openapis.org/specification/security.html#http-authentication",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                OpenIdConnectDefaults.AuthenticationScheme => new OpenApiSecurityScheme
                {
                    Description = "https://learn.openapis.org/specification/security.html#openid-connect",
                    Type = SecuritySchemeType.OpenIdConnect,
                    OpenIdConnectUrl = new Uri($"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/v2.0/.well-known/openid-configuration"),
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = OpenIdConnectDefaults.AuthenticationScheme
                    }
                },
                $"{ApiKeyDefaults.AuthenticationScheme}-Header" => new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Description = "https://learn.openapis.org/specification/security.html#api-keys",
                    Name = "Ocp-Apim-Subscription-Key",
                    In = ParameterLocation.Header,
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = $"{ApiKeyDefaults.AuthenticationScheme}-Header"
                    }
                },
                $"{ApiKeyDefaults.AuthenticationScheme}-Query" => new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Description = "https://learn.openapis.org/specification/security.html#api-keys",
                    Name = "subscriptionKey",
                    In = ParameterLocation.Query,
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = $"{ApiKeyDefaults.AuthenticationScheme}-Query"
                    }
                },
                _ => null
            };

            if (securityScheme != null)
            {
                document.Components.SecuritySchemes.Add(securityScheme.Reference.Id, securityScheme);
                document.SecurityRequirements.Add(new OpenApiSecurityRequirement() { { securityScheme, new List<string>() } });
            }
        }
    }
}
