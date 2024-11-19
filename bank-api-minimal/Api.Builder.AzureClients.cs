static partial class ApiBuilder
{
    public static IHostApplicationBuilder AddAzureClients(this IHostApplicationBuilder builder)
    {
        builder.AddAzureBlobClient("BankStorage", options =>
        {
            options.DisableHealthChecks = true;
        });

        return builder;
    }
}