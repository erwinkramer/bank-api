using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

public static partial class ApiBuilder
{
    public static IServiceCollection AddRateLimitServices(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
            options.AddPolicy("fixed", httpContext =>
            {
                httpContext.Response.Headers["X-Rate-Limit-Limit"] = GlobalConfiguration.ApiSettings!.FixedWindowRateLimit.PermitLimit.ToString();

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.Request.Headers.Host.ToString(),
                    factory:
                        partition => GlobalConfiguration.ApiSettings!.FixedWindowRateLimit
                );
            }).OnRejected = async (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();
                    await context.HttpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("Rate limit reached. Please try again later."));
                }
                return;
            };
        });

        return services;
    }
}