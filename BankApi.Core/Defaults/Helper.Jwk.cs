using Jose;
using System.Security.Cryptography;

public static class JwkHelper
{
    public static WebApplication MapJwk(this WebApplication app, out Jwk jwk)
    {
        var ecSigner = ECDsa.Create(ECCurve.NamedCurves.nistP521); // Typically, you'd load this from a secure location and not create a new one each time

        jwk = new Jwk(ecSigner, false)
        {
            KeyId = "bank-api-2025-1"
        };

        var keySet = new JwkSet(jwk);

        app.MapGet("/.well-known/jwks.json", () => TypedResults.Ok(keySet.ToDictionary()))
            .ExcludeFromDescription()
            .AllowAnonymous();

        return app;
    }
}
