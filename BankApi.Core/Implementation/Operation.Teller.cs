using Amazon.S3;
using Amazon.S3.Model;
using Dapr.SecretsManagement;
using DownstreamClients.GitHub;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

public class TellerOperation
{
    private const string ReportsBucketName = "reports";

    public static async Task<Results<Ok<Teller>, NotFound, UnprocessableEntity>> GetBankTeller(GitHubClient client, ClaimsPrincipal user, ILogger logger, CancellationToken token = default)
    {
        LogMessage.LogAccessMessage(logger, "teller", new()
        {
            AuthenticationType = user.Identity?.AuthenticationType,
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)
        });

        var release = await client.Repos["dotnet"]["runtime"].Releases.Latest.GetAsync(cancellationToken: token);

        return release?.Author?.HtmlUrl is string authorGitHubUrl
            ? TypedResults.Ok(new Teller() { GitHubProfile = new(authorGitHubUrl) })
            : TypedResults.NotFound();
    }

    public static async Task<Results<Ok<TellerReportList>, NotFound, UnprocessableEntity>> GetBankTellerReports(IAmazonS3 s3Client, CancellationToken token = default)
    {
        var response = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = ReportsBucketName
        }, token);

        TellerReportList reports = new();
        foreach (S3Object s3Object in response.S3Objects)
        {
            reports.data.Add(new() { Name = s3Object.Key });
            reports.count++;
        }

        return reports is { count: > 0 }
            ? TypedResults.Ok(reports)
            : TypedResults.NotFound();
    }

    public static async Task<Results<Ok<TellerReportList>, NotFound, UnprocessableEntity>> GetBankTellerSecretReports(DaprSecretsManagementClient daprClient, CancellationToken token = default)
    {
        var response = await daprClient.GetSecretAsync("secretstore-bankapi", "banksecret", cancellationToken: token);

        TellerReportList reports = new();
        foreach (var secret in response)
        {
            reports.data.Add(new() { Name = secret.Value });
            reports.count++;
        }

        return reports is { count: > 0 }
            ? TypedResults.Ok(reports)
            : TypedResults.NotFound();
    }
}
