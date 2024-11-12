using DownstreamClients.GitHub;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

static partial class ApiBuilder
{
    public static IServiceCollection AddDownstreamApiServices(this IServiceCollection services)
    {
        services.AddKiotaHandlers();
        services.AddHttpClient<GitHubClient>().AddTypedClient((client, sp) =>
        {
            var requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: client);
            return new GitHubClient(requestAdapter);
        }).AttachKiotaHandlers();

        return services;
    }
}