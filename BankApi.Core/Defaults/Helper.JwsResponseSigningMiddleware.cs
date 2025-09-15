using Jose;
using System.Security.Cryptography;

/// <summary>
/// <para> This middleware class signs the response body of each HTTP response using JSON Web Signature (JWS) with ECDSA.</para>
/// </summary>
public class JwsResponseSigningMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ECDsa _ecSigner;
    private static readonly string[] headerCritValue = ["iat", "alg"];

    public JwsResponseSigningMiddleware(RequestDelegate next, ECDsa ecSigner)
    {
        _next = next;
        _ecSigner = ecSigner;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip signing for scalar (UI) requests
        if (context.Request.Path.StartsWithSegments("/scalar"))
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
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "crit", headerCritValue }
        };

        // Sign the response body using ECDSA
        string jws = JWT.EncodeBytes(
            responseBytes,
            _ecSigner,
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