using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using Microsoft.EntityFrameworkCore;

public class BankEventOutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    ILogger<BankEventOutboxBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 5;
    internal const int MaxAttempts = 10;
    internal static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(300);
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

    internal async Task ProcessBatch(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankDb>();
        var eventFormatter = GlobalConfiguration.JsonEventFormatter!;

        var httpClient = httpClientFactory.CreateClient("bank-outbox-publisher");
        var leaseDuration = httpClient.Timeout * BatchSize;

        var claimedMessages = await ClaimPendingMessages(dbContext, leaseDuration, cancellationToken);

        if (claimedMessages.Count == 0)
        {
            return;
        }

        foreach (var outboxEntry in claimedMessages)
        {
            if (outboxEntry.NextAttemptAt <= DateTimeOffset.UtcNow)
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
                var response = await httpClient.PostAsync(outboxEntry.Destination, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    outboxEntry.Status = "delivered";
                    outboxEntry.TimeDelivered = DateTimeOffset.UtcNow;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Gone) // treat 410 Gone as a signal that the message should be discarded without further retries, as per CloudEvents spec for webhooks
                {
                    outboxEntry.Status = "gone";
                    outboxEntry.LastErrorMessage = $"HTTP 410 Gone - the message will be discarded without further retries.";
                }
                else
                {
                    ScheduleNextAttempt(outboxEntry, $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase})", response.Headers.RetryAfter?.Date);
                }
            }
            catch (Exception ex)
            {
                ScheduleNextAttempt(outboxEntry, ex.Message);
            }
            finally
            {
                outboxEntry.VersionToken = Guid.NewGuid();

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

    /// <summary>
    /// Schedules the next delivery attempt with exponential backoff capped at <see cref="MaxBackoff"/>,
    /// or marks the entry as permanently failed once <see cref="MaxAttempts"/> attempts have been exhausted.
    /// </summary>
    internal static void ScheduleNextAttempt(BankEventOutboxModel outboxEntry, string errorMessage, DateTimeOffset? retryAfter = null)
    {
        if (outboxEntry.AttemptCount >= MaxAttempts)
        {
            outboxEntry.Status = "failed";
            outboxEntry.LastErrorMessage = $"{errorMessage} (giving up after {MaxAttempts} attempts)";
        }
        else
        {
            outboxEntry.Status = "pending";
            outboxEntry.LastErrorMessage = errorMessage;
            var backoffSeconds = Math.Min(MaxBackoff.TotalSeconds, Math.Pow(2, outboxEntry.AttemptCount));
            outboxEntry.NextAttemptAt = retryAfter ?? DateTimeOffset.UtcNow.AddSeconds(backoffSeconds);
        }
    }

    private async Task<List<BankEventOutboxModel>> ClaimPendingMessages(BankDb dbContext, TimeSpan leaseDuration, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var candidates = await dbContext.Outbox
            .Where(x => (x.Status == "pending" || x.Status == "processing") && x.NextAttemptAt <= now)
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
            candidate.NextAttemptAt = now.Add(leaseDuration);
            candidate.VersionToken = Guid.NewGuid();
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
