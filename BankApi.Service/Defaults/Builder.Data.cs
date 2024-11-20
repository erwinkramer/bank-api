using Microsoft.EntityFrameworkCore;

static partial class ApiBuilder
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddDbContext<BankDb>(options =>
        {
            options.UseInMemoryDatabase(GlobalConfiguration.ApiSettings!.DatabaseName);
        });
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