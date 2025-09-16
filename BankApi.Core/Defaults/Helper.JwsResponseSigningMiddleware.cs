using Jose;
using System.Security.Cryptography;

/// <summary>
/// <para> This middleware class signs the response body of each HTTP response using JSON Web Signature (JWS) with ECDSA.</para>
/// </summary>
public class JwsResponseSigningMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Jwk _jwk;
    private static readonly string[] headerCritValue = ["kid", "alg"];
    private static readonly string[] pathsToSkip = ["/scalar", "/openapi", "/health"];

    public JwsResponseSigningMiddleware(RequestDelegate next, Jwk jwk)
    {
        _next = next;
        _jwk = jwk;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (pathsToSkip.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;

        await using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        byte[] responseBytes = memoryStream.ToArray();

        var extraHeaders = new Dictionary<string, object>
        {
            { "crit", headerCritValue },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "kid", _jwk.KeyId }
        };

        // Sign the response body using ECDSA
        string jws = JWT.EncodeBytes(
            responseBytes,
            _jwk,
            JwsAlgorithm.ES512,
            extraHeaders,
            options: new JwtOptions { DetachPayload = true });

        context.Response.Headers["X-JWS-Signature"] = jws;

        // Verify the signature (for debugging purposes)
        //JWT.VerifyBytes(jws, new Jwk(_ecSigner, false), payload: responseBytes);

        // Replay original response body
        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBodyStream, context.RequestAborted);

        context.Response.Body = originalBodyStream;
    }
}