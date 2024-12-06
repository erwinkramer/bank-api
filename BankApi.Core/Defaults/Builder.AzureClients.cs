using Azure.Storage.Blobs;

public static partial class ApiBuilder
{
    public static IHostApplicationBuilder AddAzureClients(this IHostApplicationBuilder builder)
    {
        builder.AddAzureBlobClient("BankStorage", options =>
        {
            options.DisableHealthChecks = false;
        });

        return builder;
    }

    public static async void ProvisionAzureStorage(this IServiceProvider provider)
    {
        var container = provider.GetRequiredService<BlobServiceClient>()
                                .GetBlobContainerClient("reports");

        await container.CreateIfNotExistsAsync();
        await container.GetBlobClient("secretz.txt")
                       .UploadAsync(new BinaryData("you found the file :o"), overwrite: true);
    }
}