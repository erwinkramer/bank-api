using Jose;
using System.Security.Cryptography;

/// <summary>
/// <para> This middleware class signs the response body of each HTTP response using JSON Web Signature (JWS) with ECDSA.</para>
/// </summary>
public class JwsResponseSigningMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ECDsa _ecSigner;

    public JwsResponseSigningMiddleware(RequestDelegate next, ECDsa ecSigner)
    {
        _next = next;
        _ecSigner = ecSigner;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context); // Let the response be written to memoryStream

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

        // Create JWS header
        var extraHeaders = new Dictionary<string, object>
        {
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "crit", new[] { "iat" } }
        };

        // Sign the response body using ECDSA
        string jws = JWT.Encode(responseBody, _ecSigner, JwsAlgorithm.ES512, extraHeaders);

        // Add signature header
        context.Response.Headers["X-JWS-Signature"] = jws;

        // Write the original response back to the body
        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}
