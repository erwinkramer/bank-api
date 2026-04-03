using System.Net;
using System.Security.Claims;
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
                        httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? httpContext.Request.Headers["Ocp-Apim-Subscription-Key"].ToString(),
                    factory:
                        partition => GlobalConfiguration.ApiSettings!.FixedWindowRateLimit
                );
            }).OnRejected = async (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                    context.HttpContext.Response.ContentType = "text/plain";
                    await context.HttpContext.Response.WriteAsync("Rate limit reached. Please try again later.", cancellationToken: _);
                }
                return;
            };
        });

        return services;
    }
}