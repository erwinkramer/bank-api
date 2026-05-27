using System.Net.Http.Headers;

static class KubernetesServiceAccountExtensions
{
    public static IHttpClientBuilder AddKubernetesServiceAccountBearer(
        this IHttpClientBuilder builder,
        string tokenPath = "/var/run/secrets/bank-api/token")
    {
        return builder.AddHttpMessageHandler(() =>
            new KubernetesServiceAccountBearerHandler(tokenPath));
    }

    private sealed class KubernetesServiceAccountBearerHandler : DelegatingHandler
    {
        private readonly string _tokenPath;
        private readonly bool _hasToken;

        public KubernetesServiceAccountBearerHandler(string tokenPath)
        {
            _tokenPath = tokenPath;
            _hasToken = File.Exists(tokenPath);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_hasToken && request.Headers.Authorization is null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    (await File.ReadAllTextAsync(_tokenPath, cancellationToken)).Trim());
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}