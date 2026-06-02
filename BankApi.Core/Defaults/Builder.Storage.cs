using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

public static partial class ApiBuilder
{
    private const string ReportsBucketName = "reports";

    public static IServiceCollection AddStorageClients(this IServiceCollection services)
    {
        var storage = GlobalConfiguration.ApiSettings!.Storage;

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = storage.ServiceUrl,
                ForcePathStyle = storage.ForcePathStyle,
                UseHttp = storage.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            };

            return new AmazonS3Client(new AnonymousAWSCredentials(), s3Config);
        });

        return services;
    }

    public static async Task ProvisionStorage(this IServiceProvider provider)
    {
        var s3Client = provider.GetRequiredService<IAmazonS3>();

        if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, ReportsBucketName))
        {
            await s3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = ReportsBucketName,
                UseClientRegion = true
            });
        }

        await s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = ReportsBucketName,
            Key = "secretz.txt",
            ContentBody = "you found the file :o"
        });
    }
}