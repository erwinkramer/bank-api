
using Microsoft.Extensions.Azure;

static partial class ApiBuilder
{
    public static IServiceCollection AddAzureClients(this IServiceCollection services, IConfigurationSection azureConfig)
    {
        services.AddAzureClients(options =>
        {
            options.AddBlobServiceClient(azureConfig.GetSection("BankStorage"));
        });

        return services;
    }
}