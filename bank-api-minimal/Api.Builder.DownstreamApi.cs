using DownstreamClients.GitHub;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;

static partial class ApiBuilder
{
    public static IServiceCollection AddDownstreamApiServices(this IServiceCollection services)
    {
        services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();  // Turn on resilience by default
        });
        services.AddKiotaHandlers();
        services.AddHttpClient<GitHubClient>().AddTypedClient((client, sp) =>
        {
            var requestAdapter = new DefaultRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: client);
            return new GitHubClient(requestAdapter);
        }).AttachKiotaHandlers();

        return services;
    }
}