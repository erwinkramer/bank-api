using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DownstreamClients.GitHub;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

public class TellerOperation
{
    public static async Task<Results<Ok<Teller>, NotFound, UnprocessableEntity>> GetBankTeller(GitHubClient client, ClaimsPrincipal user, ILogger logger, CancellationToken cancellationToken)
    {
        LogMessage.LogAccessMessage(logger, "teller", new ()
        {
            AuthenticationType = user.Identity?.AuthenticationType,
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)
        });

        var release = await client.Repos["dotnet"]["runtime"].Releases.Latest.GetAsync(cancellationToken: cancellationToken);

        return release?.Author?.HtmlUrl is string authorGitHubUrl
            ? TypedResults.Ok(new Teller() { GitHubProfile = new Uri(authorGitHubUrl) })
            : TypedResults.NotFound();
    }

    public static async Task<Results<Ok<TellerReportList>, NotFound, UnprocessableEntity>> GetBankTellerReports(BlobServiceClient blobServiceClient, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("reports");

        TellerReportList reports = new();
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            reports.data.Add(new () { Name = blobItem.Name });
            reports.count++;
        }

        return reports is { count: > 0 }
            ? TypedResults.Ok(reports)
            : TypedResults.NotFound();
    }
}
