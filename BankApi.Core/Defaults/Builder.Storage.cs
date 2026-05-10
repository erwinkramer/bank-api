using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

public static partial class ApiBuilder
{
    private const string ReportsBucketName = "reports";

    public static IHostApplicationBuilder AddStorageClients(this IHostApplicationBuilder builder)
    {
        var storageOptions = builder.Configuration.GetRequiredSection("Storage").Get<S3StorageOptions>()!;

        builder.Services.AddSingleton<IAmazonS3>(_ =>
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = storageOptions.ServiceUrl,
                ForcePathStyle = storageOptions.ForcePathStyle,
                UseHttp = storageOptions.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            };

            return new AmazonS3Client(new AnonymousAWSCredentials(), s3Config);
        });

        return builder;
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

    public sealed class S3StorageOptions
    {
        public string ServiceUrl { get; set; } = "";
        public bool ForcePathStyle { get; set; } = true;
    }
}