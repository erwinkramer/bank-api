using Microsoft.EntityFrameworkCore;

public static partial class ApiBuilder
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnectionString = configuration.GetConnectionString("bank-api-db");
        var useInMemoryDatabase = string.IsNullOrWhiteSpace(postgresConnectionString);

        services.AddDbContext<BankDb>(options =>
        {
            if (useInMemoryDatabase)
                options.UseInMemoryDatabase("bank-api-db");
            else
                options.UseNpgsql(postgresConnectionString);
        });
        services.AddHttpClient("bank-outbox-publisher", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddKubernetesServiceAccountBearer();

        services.AddHostedService<BankEventOutboxBackgroundService>();

        // Consider adding the distributed cache layer (L2) for hybrid cache:
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/caching#configure-distributed-cache
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = GlobalConfiguration.ApiSettings!.Cache;
        });
        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }

    public static void EnsureDataServicesCreated(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<BankDb>().Database.EnsureCreated();
    }
}