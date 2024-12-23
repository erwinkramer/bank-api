using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

public static partial class ApiBuilder
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        var apiKeyEvents = new ApiKeyEvents
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
                options.TokenValidationParameters = GlobalConfiguration.ApiSettings!.TokenValidation;

                //SignatureValidator added because of issue in library: https://github.com/dotnet/aspnetcore/issues/52075#issuecomment-2037161895
                options.TokenValidationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);
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