using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

public static partial class ApiBuilder
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IHostEnvironment environment)
    {
        ApiKeyEvents apiKeyEvents = new()
        {
            OnValidateKey = context =>
            {
                if (context.ApiKey == "Lifetime Subscription")
                {
                    context.ValidationSucceeded();
                }
                else
                {
                    context.ValidationFailed();
                }
                return Task.CompletedTask;
            }
        };

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddApiKeyInHeader($"{ApiKeyDefaults.AuthenticationScheme}-Header", options =>
            {
                options.KeyName = "Ocp-Apim-Subscription-Key";
                options.Realm = "API";
                options.Events = apiKeyEvents;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = $"https://login.microsoftonline.com/{GlobalConfiguration.ApiSettings!.EntraId.TenantId}/v2.0";
                options.TokenValidationParameters = GlobalConfiguration.ApiSettings!.TokenValidation;

                if (environment.IsDevelopment())
                {
                    // In development, we accept any valid token without validating the signature (via the authority)
                    // this is to simplify local development with self-signed tokens
                    options.TokenValidationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);
                }
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("bank_god", policy =>
            {
                policy.RequireAuthenticatedUser(); // an anonymous user can still provide roles and claims if we do not add this
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole("banker", "ceo");
            });
            options.AddPolicy("bank_subscription", policy =>
            {
                policy.RequireAuthenticatedUser(); // an anonymous user can still provide roles and claims if we do not add this
                policy.AuthenticationSchemes.Add($"{ApiKeyDefaults.AuthenticationScheme}-Header");
            });
        });

        return services;
    }
}