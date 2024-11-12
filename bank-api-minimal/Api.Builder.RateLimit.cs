using Microsoft.AspNetCore.RateLimiting;

static partial class ApiBuilder
{
    public static IServiceCollection AddRateLimitServices(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(policyName: "fixed", options =>
            {
                options.AutoReplenishment = GlobalConfiguration.ApiSettings!.FixedWindowRateLimit.AutoReplenishment;
                options.PermitLimit = GlobalConfiguration.ApiSettings.FixedWindowRateLimit.PermitLimit;
                options.Window = GlobalConfiguration.ApiSettings.FixedWindowRateLimit.Window;
                options.QueueProcessingOrder = GlobalConfiguration.ApiSettings.FixedWindowRateLimit.QueueProcessingOrder;
                options.QueueLimit = GlobalConfiguration.ApiSettings.FixedWindowRateLimit.QueueLimit;
            });
        });

        return services;
    }
}