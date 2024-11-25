using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;

static partial class ApiBuilder
{
    public static IServiceCollection AddRateLimitServices(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
            options.AddPolicy("fixed", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.Request.Headers.Host.ToString(),
                    factory:
                        partition => GlobalConfiguration.ApiSettings!.FixedWindowRateLimit
                )
            );
        });

        return services;
    }
}