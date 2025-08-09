using Microsoft.Extensions.Caching.Hybrid;

/// <summary>
/// <para> This middleware class detects token reuse and blocks subsequent use of the same token.
/// When detected as a reuse, all requests where tokens have the same "oid" claim (Object ID of the 
/// identity creating the token) will be blocked indefinitely.</para>
/// 
/// <para> Uses the "aio" claim, only found in Entra ID tokens to detect reuse. 
/// As Microsoft describes this claim: "An internal claim that's used to record data for token reuse". 
/// The claim value is always unique for each new token, so it is suitable for the functionality here.</para>
/// 
/// <para> Please configure an out-of-process secondary cache, so blacklisted entries will be more persistent.</para>
/// </summary>
public class EntraIdTokenReuseMiddleware
{
    private readonly RequestDelegate _next;

    public EntraIdTokenReuseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, HybridCache hybridCache)
    {
        // Only run for authenticated requests
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var cancellationToken = context.RequestAborted;

            var aioClaim = context.User.FindFirst("aio");

            if (aioClaim != null) // Now we're sure it's an Entra ID token
            {
                var oidClaim = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")!; // Object ID of the identity creating the token
                var oidBlockKey = $"blocked_oid:{oidClaim.Value}";

                // Check if this oid is already blocked, expiry of cache can be set to default here
                bool oidBlocked = await hybridCache.GetOrCreateAsync(
                    oidBlockKey,
                    async innerToken => false, // default if not blocked
                    cancellationToken: cancellationToken
                );

                if (oidBlocked)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: This identity is blocked due to token replay, please contact the API owner.");
                    return;
                }

                var tokenExpClaim = context.User.FindFirst("exp")!;
                var tokenExpirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(tokenExpClaim.Value));
                var tokenTimeUntilExpiration = tokenExpirationTime - DateTimeOffset.UtcNow;

                // Define the unique value to be stored for a new token.
                // Using a GUID ensures it's unique to this request.
                var uniqueId = Guid.NewGuid();

                // Atomically check if the key exists. If it doesn't, it will store the uniqueId for the token lifetime and a bit more (clock skew).
                var tokenReuseResult = await hybridCache.GetOrCreateAsync(
                    $"aio:{aioClaim.Value}",
                    async innerToken => uniqueId,
                    new() { Expiration = tokenTimeUntilExpiration + TimeSpan.FromMinutes(5) },
                    cancellationToken: cancellationToken // important, read https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0#get-and-store-cache-entries
                );

                // If the result from the cache is NOT the uniqueId we just tried to store,
                // it means a value already existed, and the token is a replay.
                if (tokenReuseResult != uniqueId)
                {
                    // Token replay detected — block all tokens with the same oid for a very long time
                    await hybridCache.SetAsync(
                        oidBlockKey,
                        true,
                        new() { Expiration = TimeSpan.FromDays(3650) },
                        cancellationToken: cancellationToken
                    );

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: The provided token has already been used. Subsequent requests with the same identity will be blocked, please contact the API owner.");
                    return;
                }
            }
        }

        await _next(context);
    }
}