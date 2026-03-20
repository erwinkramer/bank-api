public class OutboxDestinationModel
{
    private OutboxDestinationModel() { }

    public OutboxDestinationModel(string destination, string url)
    {
        Destination = destination;
        Url = url;
    }

    /// <summary>
    /// The unique identifier for the destination entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Delivery destination for this outbox entry.
    /// Should be a logical destination that the outbox processor can use to determine where to send the event (e.g. a topic name, an endpoint name, etc.)
    /// Do not use a full URL or other transport-specific address here to avoid coupling the outbox to a specific transport or delivery mechanism.
    /// </summary>
    public string? Destination { get; set; }

    /// <summary>
    /// The URL for the delivery destination.
    /// This should be a full URL that the outbox processor can use to send the event.
    /// </summary>
    public string? Url { get; set; }
}
