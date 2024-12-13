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
        LogMessage.LogAccessMessage(logger, "teller", new AccessLogModel()
        {
            AuthenticationType = user.Identity?.AuthenticationType,
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)
        });

        return await client.Repos["dotnet"]["runtime"].Releases.Latest.GetAsync(cancellationToken: cancellationToken)
            is Release teller
               ? TypedResults.Ok(teller)
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