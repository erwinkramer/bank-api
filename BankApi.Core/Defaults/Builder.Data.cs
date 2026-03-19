using Microsoft.EntityFrameworkCore;

public static partial class ApiBuilder
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddDbContext<BankDb>(options =>
        {
            options.UseInMemoryDatabase(GlobalConfiguration.ApiSettings!.DatabaseName);
        });
        services.AddHttpClient("bank-outbox-publisher", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddHostedService<BankEventOutboxBackgroundService>();
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