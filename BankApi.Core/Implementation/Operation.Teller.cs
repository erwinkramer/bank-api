using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DownstreamClients.GitHub;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

public class TellerOperation
{
    public static async Task<Results<Ok<Teller>, NotFound>> GetBankTeller(GitHubClient client, ClaimsPrincipal user, ILogger logger, CancellationToken cancellationToken)
    {
        LogMessage.LogAccessMessage(logger, "teller", new AccessLogModel()
        {
            AuthenticationType = user.Identity?.AuthenticationType,
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)
        });

        var release = await client.Repos["dotnet"]["runtime"].Releases.Latest.GetAsync(cancellationToken: cancellationToken);

        return release?.Author?.HtmlUrl
            is string authorGitHubUrl
             ? TypedResults.Ok(new Teller() { GitHubProfile = new Uri(authorGitHubUrl) })
             : TypedResults.NotFound();
    }

    public static async Task<Results<Ok<List<TellerReport>>, NotFound>> GetBankTellerReports(BlobServiceClient blobServiceClient, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("reports");

        List<TellerReport> reports = new();
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            reports.Add(new TellerReport() { Name = blobItem.Name });
        }

        if (reports.Any())
            return TypedResults.Ok(reports);

        return TypedResults.NotFound();
    }
}