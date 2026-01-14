namespace Shipping.Domain;

public class Shipment : AggregateRoot
{
    public string ShipmentNumber { get; private set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? CarrierId { get; private set; }
    public ShippingCarrier? Carrier { get; private set; }
    public ShippingProvider Provider { get; private set; }
    public string? TrackingNumber { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; }
    public Money ShippingFee { get; private set; }
    public Money? CODAmount { get; private set; }
    public PackageInfo Package { get; private set; }
    public DateTimeOffset? EstimatedDeliveryDate { get; private set; }
    public DateTimeOffset? ActualDeliveryDate { get; private set; }
    public string? Note { get; private set; }
    public string? FailureReason { get; private set; }

    private readonly List<ShipmentItem> _items = [];
    public IReadOnlyCollection<ShipmentItem> Items => _items.AsReadOnly();

    private readonly List<TrackingEvent> _trackingEvents = [];
    public IReadOnlyCollection<TrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    private Shipment() { } // EF Core

    public static Shipment Create(
        Guid orderId,
        string orderNumber,
        ShippingProvider provider,
        Address shippingAddress,
        Money shippingFee,
        Money? codAmount = null,
        PackageInfo? package = null,
        Guid? carrierId = null,
        string? note = null)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID is required");

        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new DomainException("Order number is required");

        if (shippingAddress == null)
            throw new DomainException("Shipping address is required");

        if (shippingFee == null)
            throw new DomainException("Shipping fee is required");

        var shipment = new Shipment
        {
            ShipmentNumber = GenerateShipmentNumber(),
            OrderId = orderId,
            OrderNumber = orderNumber,
            Provider = provider,
            CarrierId = carrierId,
            ShippingAddress = shippingAddress,
            ShippingFee = shippingFee,
            CODAmount = codAmount,
            Package = package ?? PackageInfo.Default(),
            Status = ShipmentStatus.Created,
            Note = note
        };

        shipment.AddTrackingEvent(
            TrackingEventType.Created,
            "Đơn vận chuyển đã được tạo");

        shipment.AddEvent(new ShipmentCreatedEvent(
            shipment.Id,
            shipment.ShipmentNumber,
            orderId,
            orderNumber,
            provider));

        return shipment;
    }

    private static string GenerateShipmentNumber()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var random = RandomCodeGenerator.Generate(4);
        return $"SHP-{timestamp}-{random}";
    }

    #region Carrier Assignment

    public void AssignCarrier(Guid carrierId)
    {
        if (carrierId == Guid.Empty)
            throw new DomainException("Carrier ID is required");

        CarrierId = carrierId;
        IncreaseVersion();
    }

    public void SetTrackingNumber(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new DomainException("Tracking number is required");

        TrackingNumber = trackingNumber;
        IncreaseVersion();
    }

    public void SetEstimatedDeliveryDate(DateTimeOffset estimatedDate)
    {
        EstimatedDeliveryDate = estimatedDate;
        IncreaseVersion();
    }

    public void UpdatePackage(PackageInfo package)
    {
        Package = package ?? throw new DomainException("Package info is required");
        IncreaseVersion();
    }

    #endregion

    #region Item Management

    public void AddItem(
        Guid productId,
        Guid variantId,
        string sku,
        string productName,
        string? variantName,
        int quantity)
    {
        if (Status != ShipmentStatus.Created && Status != ShipmentStatus.AwaitingPickup)
            throw new DomainException("Cannot modify items after pickup");

        var existingItem = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (existingItem != null)
            throw new DomainException("Item already exists in shipment");

        var item = new ShipmentItem(productId, variantId, sku, productName, variantName, quantity);
        _items.Add(item);

        IncreaseVersion();
    }

    #endregion

    #region State Machine

    /// <summary>
    /// Schedule pickup - Created -> AwaitingPickup
    /// </summary>
    public void SchedulePickup()
    {
        if (Status != ShipmentStatus.Created)
            throw new DomainException("Can only schedule pickup from Created status");

        Status = ShipmentStatus.AwaitingPickup;

        AddTrackingEvent(
            TrackingEventType.PickupScheduled,
            "Đã lên lịch lấy hàng");

        IncreaseVersion();
    }

    /// <summary>
    /// Mark picked up - AwaitingPickup -> PickedUp
    /// </summary>
    public void MarkPickedUp(string? location = null)
    {
        if (Status != ShipmentStatus.AwaitingPickup)
            throw new DomainException("Can only mark picked up from AwaitingPickup status");

        Status = ShipmentStatus.PickedUp;

        AddTrackingEvent(
            TrackingEventType.PickedUp,
            "Đã lấy hàng",
            location);

        AddEvent(new ShipmentPickedUpEvent(Id, ShipmentNumber, OrderId, TrackingNumber));
        IncreaseVersion();
    }

    /// <summary>
    /// Update transit status - PickedUp -> InTransit
    /// </summary>
    public void StartTransit(string? location = null)
    {
        if (Status != ShipmentStatus.PickedUp && Status != ShipmentStatus.InTransit)
            throw new DomainException("Can only start transit from PickedUp or InTransit status");

        Status = ShipmentStatus.InTransit;

        AddTrackingEvent(
            TrackingEventType.InTransit,
            "Đang vận chuyển",
            location);

        AddEvent(new ShipmentInTransitEvent(Id, ShipmentNumber, OrderId, location));
        IncreaseVersion();
    }

    /// <summary>
    /// Out for delivery - InTransit -> OutForDelivery
    /// </summary>
    public void OutForDelivery(string? location = null)
    {
        if (Status != ShipmentStatus.InTransit)
            throw new DomainException("Can only go out for delivery from InTransit status");

        Status = ShipmentStatus.OutForDelivery;

        AddTrackingEvent(
            TrackingEventType.OutForDelivery,
            "Đang giao hàng",
            location);

        AddEvent(new ShipmentOutForDeliveryEvent(Id, ShipmentNumber, OrderId));
        IncreaseVersion();
    }

    /// <summary>
    /// Mark delivered - OutForDelivery -> Delivered
    /// </summary>
    public void MarkDelivered(string? location = null)
    {
        if (Status != ShipmentStatus.OutForDelivery)
            throw new DomainException("Can only mark delivered from OutForDelivery status");

        Status = ShipmentStatus.Delivered;
        ActualDeliveryDate = DateTimeOffset.UtcNow;

        AddTrackingEvent(
            TrackingEventType.Delivered,
            "Đã giao hàng thành công",
            location);

        AddEvent(new ShipmentDeliveredEvent(
            Id, ShipmentNumber, OrderId, OrderNumber, ActualDeliveryDate.Value));
        IncreaseVersion();
    }

    /// <summary>
    /// Delivery failed - OutForDelivery -> DeliveryFailed
    /// </summary>
    public void MarkDeliveryFailed(string reason, string? location = null)
    {
        if (Status != ShipmentStatus.OutForDelivery)
            throw new DomainException("Can only mark delivery failed from OutForDelivery status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Failure reason is required");

        Status = ShipmentStatus.DeliveryFailed;
        FailureReason = reason;

        AddTrackingEvent(
            TrackingEventType.DeliveryFailed,
            $"Giao hàng thất bại: {reason}",
            location);

        AddEvent(new ShipmentDeliveryFailedEvent(Id, ShipmentNumber, OrderId, reason));
        IncreaseVersion();
    }

    /// <summary>
    /// Retry delivery - DeliveryFailed -> InTransit
    /// </summary>
    public void RetryDelivery()
    {
        if (Status != ShipmentStatus.DeliveryFailed)
            throw new DomainException("Can only retry from DeliveryFailed status");

        Status = ShipmentStatus.InTransit;
        FailureReason = null;

        AddTrackingEvent(
            TrackingEventType.InTransit,
            "Đang giao lại");

        IncreaseVersion();
    }

    /// <summary>
    /// Return shipment - DeliveryFailed/InTransit -> Returned
    /// </summary>
    public void Return(string? reason = null)
    {
        if (Status != ShipmentStatus.DeliveryFailed && Status != ShipmentStatus.InTransit)
            throw new DomainException("Can only return from DeliveryFailed or InTransit status");

        Status = ShipmentStatus.Returned;

        AddTrackingEvent(
            TrackingEventType.Returned,
            reason ?? "Đã hoàn trả hàng");

        AddEvent(new ShipmentReturnedEvent(Id, ShipmentNumber, OrderId, reason));
        IncreaseVersion();
    }

    /// <summary>
    /// Cancel shipment - Created/AwaitingPickup -> Cancelled
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status != ShipmentStatus.Created && Status != ShipmentStatus.AwaitingPickup)
            throw new DomainException("Can only cancel from Created or AwaitingPickup status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Cancellation reason is required");

        Status = ShipmentStatus.Cancelled;

        AddTrackingEvent(
            TrackingEventType.Cancelled,
            $"Đã hủy: {reason}");

        AddEvent(new ShipmentCancelledEvent(Id, ShipmentNumber, OrderId, reason));
        IncreaseVersion();
    }

    #endregion

    #region Tracking

    public void AddTrackingEvent(
        TrackingEventType eventType,
        string description,
        string? location = null,
        DateTimeOffset? timestamp = null,
        string? externalEventId = null,
        string? rawData = null)
    {
        // Avoid duplicate external events
        if (!string.IsNullOrEmpty(externalEventId) &&
            _trackingEvents.Any(e => e.ExternalEventId == externalEventId))
        {
            return;
        }

        var trackingEvent = new TrackingEvent(
            eventType, description, location, timestamp, externalEventId, rawData);
        trackingEvent.SetShipmentId(Id);

        _trackingEvents.Add(trackingEvent);
    }

    public void SyncFromCarrier(IEnumerable<TrackingEvent> externalEvents)
    {
        foreach (var evt in externalEvents.OrderBy(e => e.Timestamp))
        {
            AddTrackingEvent(
                evt.EventType,
                evt.Description,
                evt.Location,
                evt.Timestamp,
                evt.ExternalEventId,
                evt.RawData);
        }

        IncreaseVersion();
    }

    public TrackingEvent? GetLatestTrackingEvent()
    {
        return _trackingEvents.OrderByDescending(e => e.Timestamp).FirstOrDefault();
    }

    #endregion

    #region Helpers

    public bool IsTerminalStatus()
    {
        return Status == ShipmentStatus.Delivered ||
               Status == ShipmentStatus.Returned ||
               Status == ShipmentStatus.Cancelled;
    }

    public bool CanCancel()
    {
        return Status == ShipmentStatus.Created ||
               Status == ShipmentStatus.AwaitingPickup;
    }

    #endregion
}
