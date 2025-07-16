using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DownstreamClients.GitHub;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class TellerOperation
{
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

    public static async Task<Results<Ok<TellerReportList>, NotFound, UnprocessableEntity>> GetBankTellerReports([FromServices] BlobServiceClient blobServiceClient, CancellationToken token = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("reports");

        TellerReportList reports = new();
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(cancellationToken: token))
        {
            reports.data.Add(new() { Name = blobItem.Name });
            reports.count++;
        }

        return reports is { count: > 0 }
            ? TypedResults.Ok(reports)
            : TypedResults.NotFound();
    }
}
