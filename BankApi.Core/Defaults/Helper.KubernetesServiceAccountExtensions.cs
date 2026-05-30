using System.Net.Http.Headers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

static class KubernetesServiceAccountExtensions
{
    internal const string ClientNameTokenAssertion = "token";

    public static IHttpClientBuilder AddKubernetesServiceAccountBearer(
        this IHttpClientBuilder builder,
        string tokenPath = "/var/run/secrets/bank-api/token",
        string tokenEndpoint = "http://keycloak-service.infra-keycloak.svc:8080/realms/bank/protocol/openid-connect/token")
    {
        builder.Services.AddHttpClient(ClientNameTokenAssertion);

        return builder.AddHttpMessageHandler(sp =>
            new KubernetesServiceAccountBearerHandler(
                sp.GetRequiredService<IHttpClientFactory>(),
                tokenPath,
                tokenEndpoint));
    }
}

internal sealed class KubernetesServiceAccountBearerHandler(
    IHttpClientFactory httpClientFactory,
    string tokenPath,
    string tokenEndpoint) : DelegatingHandler
{
    private readonly bool _hasToken = File.Exists(tokenPath);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_hasToken && request.Headers.Authorization is null)
        {
            var token = await GetKeycloakAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetKeycloakAccessTokenAsync(CancellationToken cancellationToken)
    {
        var assertion = await File.ReadAllTextAsync(tokenPath, cancellationToken);

        var tokenRequest = new OpenIdConnectMessage
        {
            GrantType = OpenIdConnectGrantTypes.ClientCredentials,
            ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            ClientAssertion = assertion
        };

        var tokenClient = httpClientFactory.CreateClient(KubernetesServiceAccountExtensions.ClientNameTokenAssertion);

        using var response = await tokenClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(tokenRequest.Parameters),
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = new OpenIdConnectMessage(responseContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"{tokenResponse.Error}: {tokenResponse.ErrorDescription}");
        }

        return tokenResponse.AccessToken
            ?? throw new InvalidOperationException("Keycloak response did not contain access_token.");
    }
}
