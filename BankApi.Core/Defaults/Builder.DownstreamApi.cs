using DownstreamClients.GitHub;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;

public static partial class ApiBuilder
{
    public static IServiceCollection AddDownstreamApiServices(this IServiceCollection services)
    {
        services.AddServiceDiscovery();
        services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();  // Turn on resilience by default
            http.AddServiceDiscovery(); // Turn on service discovery by default
        });
        services.AddKiotaHandlers();
        services.AddHttpClient<GitHubClient>().AddTypedClient((client, sp) =>
        {
            DefaultRequestAdapter requestAdapter = new (new AnonymousAuthenticationProvider(), httpClient: client)
            {
                BaseUrl = "https://github" // resolved by service discovery
            };

            return new GitHubClient(requestAdapter);
        }).AttachKiotaHandlers();

        return services;
    }
}