using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class BankEventOutboxModel
{
    private BankEventOutboxModel() { }

    public BankEventOutboxModel(Guid bankId, EntityState eventSubtype)
    {
        var now = DateTimeOffset.UtcNow;

        BankId = bankId;
        EventSubtype = eventSubtype.ToString().ToLower();
        TimeCreated = now;
        TimeUntilAttempt = now;
        LockedUntil = now;
    }

    /// <summary>
    /// The unique identifier for the outbox entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The identifier for the bank.
    /// </summary>
    public Guid? BankId { get; set; }

    /// <summary>
    /// The subtype of the event.
    /// </summary>
    public string? EventSubtype { get; set; }

    /// <summary>
    /// The time when the event was created.
    /// </summary>
    public DateTimeOffset TimeCreated { get; private set; }

    /// <summary>
    /// The time when the event was delivered.
    /// </summary>
    public DateTimeOffset? TimeDelivered { get; set; }

    /// <summary>
    /// Last error message if the event failed to be delivered.
    /// </summary>
    public string? LastErrorMessage { get; set; }

    /// <summary>
    /// Delivery destination for this outbox entry.
    /// </summary>
    public required OutboxDestinationModel Destination { get; set; }

    /// <summary>
    /// Delivery status for this outbox entry.
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Number of delivery attempts for this outbox entry.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Identifier of the worker that currently holds the lease.
    /// </summary>
    public string? LockedBy { get; set; }

    /// <summary>
    /// Lease expiration timestamp.
    /// </summary>
    public DateTimeOffset LockedUntil { get; set; }

    /// <summary>
    /// Last time delivery was attempted.
    /// </summary>
    public DateTimeOffset? TimeLastAttempted { get; set; }

    /// <summary>
    /// The time until the next delivery attempt is allowed for this outbox entry.
    /// </summary>
    public DateTimeOffset TimeUntilAttempt { get; set; }

    /// <summary>
    /// Optimistic concurrency token used during claim/update.
    /// </summary>
    [ConcurrencyCheck]
    public Guid ClaimVersion { get; set; } = Guid.NewGuid();
}
