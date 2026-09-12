using System.Net;
using CloudNative.CloudEvents.SystemTextJson;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace BankApi.Tests.Outbox;

public class BankEventOutboxTests
{
    [Before(Class)]
    public static Task CreateContext()
    {
        GlobalConfiguration.JsonEventFormatter = new JsonEventFormatter(new JsonSerializerOptions(), new JsonDocumentOptions());

        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        GlobalConfiguration.ApiSettings = config.GetRequiredSection("ApiSettings").Get<GlobalConfiguration.SettingsModel>()!;

        return Task.CompletedTask;
    }

    [Test]
    public async Task SuccessfulDeliveryIsMarkedDelivered()
    {
        var (service, db) = CreateService(HttpStatusCode.OK, attemptCount: 0);

        await service.ProcessBatch(default);

        // Query the store rather than Local: ProcessBatch runs against its own scoped context instance.
        var entry = await db.Outbox.AsNoTracking().SingleAsync();
        await Assert.That(entry.Status).IsEqualTo("delivered");
        await Assert.That(entry.TimeDelivered).IsNotNull();
    }

    [Test]
    public async Task FailedDeliveryIsScheduledForRetryWithExponentialBackoff()
    {
        var (service, db) = CreateService(HttpStatusCode.InternalServerError, attemptCount: 0);

        await service.ProcessBatch(default);

        var entry = await db.Outbox.AsNoTracking().SingleAsync();
        await Assert.That(entry.Status).IsEqualTo("pending");
        await Assert.That(entry.AttemptCount).IsEqualTo(1);
        await Assert.That(entry.LastErrorMessage).Contains("500");

        var backoffSeconds = (entry.NextAttemptAt - DateTimeOffset.UtcNow).TotalSeconds;
        await Assert.That(backoffSeconds).IsBetween(1, 4); // First retry: 2^1 = 2 seconds.
    }

    [Test]
    public async Task BackoffIsCappedAtMaxBackoff()
    {
        // This is attempt number MaxAttempts - 1; the uncapped backoff would be 2^(MaxAttempts - 1) = 512 seconds.
        var (service, db) = CreateService(HttpStatusCode.InternalServerError, attemptCount: BankEventOutboxBackgroundService.MaxAttempts - 2);

        await service.ProcessBatch(default);

        var entry = await db.Outbox.AsNoTracking().SingleAsync();
        await Assert.That(entry.Status).IsEqualTo("pending");

        var backoffSeconds = (entry.NextAttemptAt - DateTimeOffset.UtcNow).TotalSeconds;
        await Assert.That(backoffSeconds).IsBetween(BankEventOutboxBackgroundService.MaxBackoff.TotalSeconds - 5, BankEventOutboxBackgroundService.MaxBackoff.TotalSeconds + 5);
    }

    [Test]
    public async Task EntryIsFailedPermanentlyAfterMaxAttempts()
    {
        var (service, db) = CreateService(HttpStatusCode.InternalServerError, attemptCount: BankEventOutboxBackgroundService.MaxAttempts - 1);

        await service.ProcessBatch(default);

        var entry = await db.Outbox.AsNoTracking().SingleAsync();
        await Assert.That(entry.Status).IsEqualTo("failed");
        await Assert.That(entry.AttemptCount).IsEqualTo(BankEventOutboxBackgroundService.MaxAttempts);
        await Assert.That(entry.LastErrorMessage).Contains("giving up");
    }

    /// <summary>
    /// Builds the service collection with an in-memory database and a fake HTTP handler, then seeds a single outbox entry that is immediately due for processing.
    /// </summary>
    private static (BankEventOutboxBackgroundService Service, BankDb Db) CreateService(HttpStatusCode responseStatusCode, int attemptCount)
    {
        // Captured outside the options lambda: it runs once per context, so a fresh value per call would isolate every context into its own database.
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection()
            .AddDbContext<BankDb>(options => options.UseInMemoryDatabase(dbName));

        // The service publishes via the named "bank-outbox-publisher" client; the fake handler stands in for the real one.
        services.AddHttpClient("bank-outbox-publisher")
            .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler(responseStatusCode));
        services.AddSingleton<BankEventOutboxBackgroundService>();

        var provider = services.BuildServiceProvider();

        // The scope is kept alive so the scoped BankDb instance remains usable for the duration of the test.
        var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankDb>();

        db.Outbox.Add(new BankEventOutboxModel(Guid.NewGuid(), EntityState.Added)
        {
            Destination = "https+http://city",
            AttemptCount = attemptCount,
            NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(-1) // The entry is immediately due for processing.
        });
        db.SaveChanges();
        db.ChangeTracker.Clear(); // Subsequent queries then read current data from the shared in-memory store.

        return (provider.GetRequiredService<BankEventOutboxBackgroundService>(), db);
    }
}

/// <summary>Stands in for the real HTTP handler, returning a fixed status code without any network activity.</summary>
public class FakeHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage(statusCode));
}

