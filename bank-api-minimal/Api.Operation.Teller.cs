using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DownstreamClients.GitHub;
using DownstreamClients.GitHub.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

public class TellerOperation
{
    public static async Task<Results<Ok<Release>, NotFound>> GetBankTeller(GitHubClient client, ClaimsPrincipal user, ILogger logger, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Hi, {user.Identity?.Name}. What's up?");
        return await client.Repos["dotnet"]["runtime"].Releases.Latest.GetAsync(cancellationToken: cancellationToken)
            is Release teller
               ? TypedResults.Ok(teller)
               : TypedResults.NotFound();
    }

    public static async Task<Results<Ok<List<BlobItem>>, NotFound>> GetBankTellerReports(BlobServiceClient blobServiceClient, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("reports");

        List<BlobItem> reports = new();
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            reports.Add(blobItem);
        }

        if (reports.Any())
            return TypedResults.Ok(reports);

        return TypedResults.NotFound();
    }
}