using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using Microsoft.EntityFrameworkCore;

public class BankEventOutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    ILogger<BankEventOutboxBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(60); // make this longer than the expected max processing time to reduce chances of multiple workers processing the same message concurrently
    private const int BatchSize = 50;
    private readonly string workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatch(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processor failed while processing a batch.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatch(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankDb>();
        var eventFormatter = GlobalConfiguration.JsonEventFormatter!;

        var claimedMessages = await ClaimPendingMessages(dbContext, cancellationToken);

        if (claimedMessages.Count == 0)
        {
            return;
        }

        var httpClient = httpClientFactory.CreateClient("bank-outbox-publisher");

        foreach (var outboxEntry in claimedMessages)
        {
            if (outboxEntry.LockedUntil <= DateTimeOffset.UtcNow)
            {
                break;
            }

            try
            {
                outboxEntry.AttemptCount++;
                outboxEntry.TimeLastAttempted = DateTimeOffset.UtcNow;

                var bankEvent = new BankEvent(outboxEntry.EventSubtype!, outboxEntry.TimeCreated)
                {
                    Data = new()
                    {
                        BankId = outboxEntry.BankId
                    }
                };

                var content = bankEvent.CloudEvent.ToHttpContent(ContentMode.Structured, eventFormatter);
                var response = await httpClient.PostAsync("https://webhook.site/7a591ee4-4e65-4b84-94a8-f7883c788dd2", content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    outboxEntry.TimeDelivered = DateTimeOffset.UtcNow;
                    outboxEntry.Status = "delivered";
                    outboxEntry.LastErrorMessage = null;
                }
                else
                {
                    outboxEntry.Status = "pending";
                    outboxEntry.LastErrorMessage = $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase})";
                }
            }
            catch (Exception ex)
            {
                outboxEntry.Status = "pending";
                outboxEntry.LastErrorMessage = ex.Message;
            }
            finally
            {
                outboxEntry.LockedBy = null;
                outboxEntry.LockedUntil = null;
                outboxEntry.ClaimVersion = Guid.NewGuid();

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogWarning(ex, "Outbox processor encountered a concurrency conflict while finalizing message delivery for outbox id {OutboxId}.", outboxEntry.Id);
                }
            }
        }
    }

    private async Task<List<BankEventOutboxModel>> ClaimPendingMessages(BankDb dbContext, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var candidates = await dbContext.Outbox
            .Where(x => x.Status == "pending" || (x.Status == "processing" && x.LockedUntil < now))
            .OrderBy(x => x.TimeCreated)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return candidates;
        }

        foreach (var candidate in candidates)
        {
            candidate.Status = "processing";
            candidate.LockedBy = workerId;
            candidate.LockedUntil = now.Add(LeaseDuration);
            candidate.ClaimVersion = Guid.NewGuid();
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return [];
        }

        return candidates;
    }
}
