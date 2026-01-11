namespace Orders.Domain;

public class Order : AggregateRoot
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Address ShippingAddress { get; private set; }
    public Address? BillingAddress { get; private set; }

    // Payment
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public Guid? PaymentId { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    // Shipping
    public ShippingMethod ShippingMethod { get; private set; }
    public Guid? ShippingId { get; private set; }

    // Pricing
    public Money SubTotal { get; private set; }
    public Money ShippingFee { get; private set; }
    public Money TotalDiscount { get; private set; }
    public Money GrandTotal { get; private set; }

    // Status
    public OrderStatus Status { get; private set; }
    public string? CustomerNote { get; private set; }
    public string? AdminNote { get; private set; }
    public string? CancellationReason { get; private set; }

    // Collections
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderDiscount> _discounts = [];
    public IReadOnlyCollection<OrderDiscount> Discounts => _discounts.AsReadOnly();

    private readonly List<OrderStatusHistory> _statusHistory = [];
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private Order() { } // EF Core

    public static Order Create(
        Guid customerId,
        Address shippingAddress,
        PaymentMethod paymentMethod,
        ShippingMethod shippingMethod,
        string? customerNote = null)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty");

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            ShippingAddress = shippingAddress ?? throw new DomainException("Shipping address is required"),
            PaymentMethod = paymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            ShippingMethod = shippingMethod,
            SubTotal = new Money(0),
            ShippingFee = new Money(0),
            TotalDiscount = new Money(0),
            GrandTotal = new Money(0),
            Status = OrderStatus.Draft,
            CustomerNote = customerNote
        };

        return order;
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var random = RandomCodeGenerator.Generate(4);
        return $"ORD-{timestamp}-{random}";
    }

    #region Item Management

    public void AddItem(
        Guid productId,
        Guid variantId,
        string sku,
        string productName,
        string variantName,
        string? thumbnail,
        Money unitPrice,
        int quantity)
    {
        EnsureCanModify();

        var existingItem = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = new OrderItem(
                productId, variantId, sku, productName,
                variantName, thumbnail, unitPrice, quantity);
            _items.Add(item);
        }

        RecalculateTotals();
        IncreaseVersion();
    }

    public void RemoveItem(Guid variantId)
    {
        EnsureCanModify();

        var item = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotals();
            IncreaseVersion();
        }
    }

    public void UpdateItemQuantity(Guid variantId, int newQuantity)
    {
        EnsureCanModify();

        var item = _items.FirstOrDefault(i => i.VariantId == variantId)
            ?? throw new DomainException("Item not found");

        item.UpdateQuantity(newQuantity);
        RecalculateTotals();
        IncreaseVersion();
    }

    #endregion

    #region Discount Management

    public void ApplyCoupon(
        Guid couponId, 
        string couponCode, 
        string description, 
        Money discountAmount,
        Dictionary<Guid, Money>? itemAllocations = null)
    {
        EnsureCanModify();

        // Remove existing coupon discount
        var existingCoupon = _discounts.FirstOrDefault(d => d.Source == DiscountSourceType.Coupon);
        if (existingCoupon != null)
            _discounts.Remove(existingCoupon);

        var discount = OrderDiscount.FromCoupon(couponId, couponCode, description, discountAmount);
        _discounts.Add(discount);

        // Apply allocations to items
        if (itemAllocations != null)
        {
            ApplyAllocations(itemAllocations);
        }

        RecalculateTotals();
        IncreaseVersion();
    }

    public void RemoveCoupon()
    {
        EnsureCanModify();

        var couponDiscount = _discounts.FirstOrDefault(d => d.Source == DiscountSourceType.Coupon);
        if (couponDiscount != null)
        {
            _discounts.Remove(couponDiscount);
            RecalculateTotals();
            IncreaseVersion();
        }
    }

    public void AddPromotionDiscount(
        Guid promotionId, 
        string description, 
        Money discountAmount,
        Dictionary<Guid, Money>? itemAllocations = null)
    {
        EnsureCanModify();

        var discount = OrderDiscount.FromPromotion(promotionId, description, discountAmount);
        _discounts.Add(discount);

        // Apply allocations to items
        if (itemAllocations != null)
        {
            ApplyAllocations(itemAllocations);
        }

        RecalculateTotals();
        IncreaseVersion();
    }

    private void ApplyAllocations(Dictionary<Guid, Money> allocations)
    {
        foreach (var (variantId, amount) in allocations)
        {
            var item = _items.FirstOrDefault(i => i.VariantId == variantId);
            if (item != null)
            {
                // Accumulate discount on item
                var currentDiscount = item.DiscountAmount;
                item.ApplyDiscount(currentDiscount.Add(amount));
            }
        }
    }

    #endregion

    #region Shipping Fee

    public void SetShippingFee(Money shippingFee)
    {
        EnsureCanModify();

        ShippingFee = shippingFee ?? throw new DomainException("Shipping fee cannot be null");
        RecalculateTotals();
        IncreaseVersion();
    }

    public void SetBillingAddress(Address? billingAddress)
    {
        EnsureCanModify();
        BillingAddress = billingAddress;
        IncreaseVersion();
    }

    #endregion

    #region Order State Machine

    /// <summary>
    /// Place order and request inventory reservation
    /// Draft -> Pending
    /// </summary>
    public void PlaceOrder()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Can only place order from Draft status");

        if (!_items.Any())
            throw new DomainException("Order must have at least one item");

        ChangeStatus(OrderStatus.Pending);

        AddEvent(new OrderPlacedEvent(
            Id, OrderNumber, CustomerId,
            _items.Select(i => new OrderItemDto(i.VariantId, i.Sku, i.Quantity)).ToList()));
    }

    /// <summary>
    /// Confirm inventory reservation succeeded
    /// Pending -> Reserved
    /// </summary>
    public void ConfirmReservation()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Can only confirm reservation from Pending status");

        ChangeStatus(OrderStatus.Reserved);
        AddEvent(new OrderReservationConfirmedEvent(Id, OrderNumber));
    }

    /// <summary>
    /// Inventory reservation failed
    /// Pending -> Cancelled
    /// </summary>
    public void FailReservation(string reason)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Can only fail reservation from Pending status");

        CancellationReason = reason;
        ChangeStatus(OrderStatus.Cancelled);
        AddEvent(new OrderCancelledEvent(Id, OrderNumber, reason));
    }

    /// <summary>
    /// Mark payment received (Online payment)
    /// Reserved -> Confirmed
    /// </summary>
    public void MarkPaid(Guid paymentId)
    {
        if (Status != OrderStatus.Reserved)
            throw new DomainException("Can only mark paid from Reserved status");

        if (PaymentMethod != PaymentMethod.Online)
            throw new DomainException("Use ConfirmOrder for COD orders");

        PaymentId = paymentId;
        PaymentStatus = PaymentStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;

        ChangeStatus(OrderStatus.Confirmed);
        AddEvent(new OrderConfirmedEvent(Id, OrderNumber));
    }

    /// <summary>
    /// Confirm COD order (Admin confirms)
    /// Reserved -> Confirmed
    /// </summary>
    public void ConfirmOrder(Guid? confirmedBy = null)
    {
        if (Status != OrderStatus.Reserved)
            throw new DomainException("Can only confirm order from Reserved status");

        if (PaymentMethod != PaymentMethod.COD)
            throw new DomainException("Use MarkPaid for Online payment orders");

        ChangeStatus(OrderStatus.Confirmed, changedBy: confirmedBy);
        AddEvent(new OrderConfirmedEvent(Id, OrderNumber));
    }

    /// <summary>
    /// Start shipping
    /// Confirmed -> Shipped
    /// </summary>
    public void Ship(Guid? shippingId = null)
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException("Can only ship from Confirmed status");

        ShippingId = shippingId;
        ChangeStatus(OrderStatus.Shipped);
        AddEvent(new OrderShippedEvent(Id, OrderNumber, ShippingId));
    }

    /// <summary>
    /// Mark delivered
    /// Shipped -> Delivered
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Can only deliver from Shipped status");

        ChangeStatus(OrderStatus.Delivered);
        AddEvent(new OrderDeliveredEvent(Id, OrderNumber));
    }

    /// <summary>
    /// Mark paid on delivery (COD)
    /// </summary>
    public void MarkPaidOnDelivery(Guid? paymentId = null)
    {
        if (Status != OrderStatus.Delivered && Status != OrderStatus.Shipped)
            throw new DomainException("Can only mark paid on delivery after shipping");

        if (PaymentMethod != PaymentMethod.COD)
            throw new DomainException("This method is only for COD orders");

        PaymentId = paymentId;
        PaymentStatus = PaymentStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;

        AddEvent(new OrderPaidEvent(Id, OrderNumber, GrandTotal));
        IncreaseVersion();
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    public void Cancel(string reason, Guid? cancelledBy = null)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new DomainException("Cannot cancel delivered or already cancelled order");

        var previousStatus = Status;
        CancellationReason = reason;
        ChangeStatus(OrderStatus.Cancelled, reason, cancelledBy);

        // Release inventory if was reserved
        if (previousStatus >= OrderStatus.Reserved)
        {
            AddEvent(new OrderCancelledEvent(Id, OrderNumber, reason));
        }
    }

    /// <summary>
    /// Refund order (after payment)
    /// </summary>
    public void Refund(string reason, Guid? refundedBy = null)
    {
        if (PaymentStatus != PaymentStatus.Paid)
            throw new DomainException("Can only refund paid orders");

        PaymentStatus = PaymentStatus.Refunded;
        ChangeStatus(OrderStatus.Refunded, reason, refundedBy);
        AddEvent(new OrderRefundedEvent(Id, OrderNumber, GrandTotal));
    }

    #endregion

    #region Admin

    public void SetAdminNote(string? note)
    {
        AdminNote = note;
        IncreaseVersion();
    }

    public void AssignPayment(Guid paymentId)
    {
        PaymentId = paymentId;
        IncreaseVersion();
    }

    public void AssignShipping(Guid shippingId)
    {
        ShippingId = shippingId;
        IncreaseVersion();
    }

    #endregion

    #region Private Methods

    private void EnsureCanModify()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Order can only be modified in Draft status");
    }

    private void RecalculateTotals()
    {
        var currency = _items.FirstOrDefault()?.UnitPrice.Currency ?? "VND";

        SubTotal = new Money(_items.Sum(i => i.LineTotal.Amount), currency);
        
        // Total discount is sum of Order-level discounts AND Item-level discounts
        // NOTE: OrderDiscount collection primarily tracks SOURCES. 
        // Ideally the sum of allocated item discounts should match relevant OrderDiscounts.
        // For simplicity, we calculate TotalDiscount from the DECLARED discounts in _discounts collection
        // assuming the Allocations were consistent.
        
        TotalDiscount = new Money(_discounts.Sum(d => d.Amount.Amount), currency);

        var grandTotal = SubTotal.Amount + ShippingFee.Amount - TotalDiscount.Amount;
        GrandTotal = new Money(Math.Max(0, grandTotal), currency);
    }

    private void ChangeStatus(OrderStatus newStatus, string? reason = null, Guid? changedBy = null)
    {
        var history = new OrderStatusHistory(Id, Status, newStatus, reason, changedBy);
        _statusHistory.Add(history);
        Status = newStatus;
        IncreaseVersion();
    }

    #endregion
}

public record OrderItemDto(Guid VariantId, string Sku, int Quantity);
