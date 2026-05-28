using System.Net.Http.Headers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

static class KubernetesServiceAccountExtensions
{
    private const string KeycloakTokenClientName = "keycloak";

    public static IHttpClientBuilder AddKubernetesServiceAccountBearer(
        this IHttpClientBuilder builder,
        string tokenPath = "/var/run/secrets/bank-api/token",
        string tokenEndpoint = "http://keycloak-service.infra-keycloak.svc.cluster.local:8080/realms/master/protocol/openid-connect/token")
    {
        builder.Services.AddHttpClient(KeycloakTokenClientName);

        return builder.AddHttpMessageHandler(sp =>
            new KubernetesServiceAccountBearerHandler(
                sp.GetRequiredService<IHttpClientFactory>(),
                tokenPath,
                tokenEndpoint));
    }

    private sealed class KubernetesServiceAccountBearerHandler : DelegatingHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _tokenPath;
        private readonly string _tokenEndpoint;
        private readonly bool _hasToken;

        public KubernetesServiceAccountBearerHandler(
            IHttpClientFactory httpClientFactory,
            string tokenPath,
            string tokenEndpoint)
        {
            _httpClientFactory = httpClientFactory;
            _tokenPath = tokenPath;
            _tokenEndpoint = tokenEndpoint;
            _hasToken = File.Exists(tokenPath);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_hasToken && request.Headers.Authorization is null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", await GetKeycloakAccessTokenAsync(cancellationToken));
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> GetKeycloakAccessTokenAsync(CancellationToken cancellationToken)
        {
            var assertion = await File.ReadAllTextAsync(_tokenPath, cancellationToken);

            var tokenRequest = new OpenIdConnectMessage
            {
                GrantType = OpenIdConnectGrantTypes.ClientCredentials,
                ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                ClientAssertion = assertion
            };

            var tokenClient = _httpClientFactory.CreateClient(KeycloakTokenClientName);

            using var response = await tokenClient.PostAsync(
                _tokenEndpoint,
                new FormUrlEncodedContent(tokenRequest.Parameters),
                cancellationToken);

            var tokenResponse = new OpenIdConnectMessage(
                await response.Content.ReadAsStringAsync(cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"{tokenResponse.Error}: {tokenResponse.ErrorDescription}");
            }

            return tokenResponse.AccessToken
                ?? throw new InvalidOperationException("Keycloak response did not contain access_token.");
        }
    }
}
