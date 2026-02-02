namespace Shipping.Domain;

public class TrackingEvent : BaseEntity
{
    public Guid ShipmentId { get; private set; }
    public TrackingEventType EventType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string? ExternalEventId { get; private set; }
    public string? RawData { get; private set; }

    private TrackingEvent() { } // EF Core

    public TrackingEvent(
        TrackingEventType eventType,
        string description,
        string? location = null,
        DateTimeOffset? timestamp = null,
        string? externalEventId = null,
        string? rawData = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required");

        EventType = eventType;
        Description = description;
        Location = location;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        ExternalEventId = externalEventId;
        RawData = rawData;
    }

    internal void SetShipmentId(Guid shipmentId)
    {
        ShipmentId = shipmentId;
    }
}
