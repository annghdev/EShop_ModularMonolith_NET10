namespace ShoppingCart.Domain;

public class Cart : AggregateRoot
{
    public CustomerInfo Customer { get; private set; }
    public CartStatus Status { get; private set; }

    // Applied coupon (validated at checkout)
    public string? AppliedCouponCode { get; private set; }
    public Guid? AppliedCouponId { get; private set; }

    // Calculated totals (re-calculated on demand or cached)
    public Money SubTotal { get; private set; }
    public Money TotalDiscount { get; private set; }
    public Money EstimatedTotal { get; private set; }

    // Items
    private readonly List<CartItem> _items = [];
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    // Timestamps
    public DateTimeOffset? LastActivityAt { get; private set; }

    private Cart() { } // EF Core

    #region Factory Methods

    public static Cart CreateForCustomer(Guid customerId)
    {
        return new Cart
        {
            Customer = CustomerInfo.ForRegisteredCustomer(customerId),
            Status = CartStatus.Active,
            SubTotal = new Money(0),
            TotalDiscount = new Money(0),
            EstimatedTotal = new Money(0),
            LastActivityAt = DateTimeOffset.UtcNow
        };
    }

    public static Cart CreateForGuest(string guestId)
    {
        return new Cart
        {
            Customer = CustomerInfo.ForGuest(guestId),
            Status = CartStatus.Active,
            SubTotal = new Money(0),
            TotalDiscount = new Money(0),
            EstimatedTotal = new Money(0),
            LastActivityAt = DateTimeOffset.UtcNow
        };
    }

    #endregion

    #region Item Management

    public void AddItem(
        Guid productId,
        Guid variantId,
        string sku,
        string productName,
        string variantName,
        string? thumbnail,
        Money unitPrice,
        int quantity = 1)
    {
        EnsureActive();

        var existingItem = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var item = new CartItem(
                productId, variantId, sku, productName,
                variantName, thumbnail, unitPrice, quantity);
            _items.Add(item);

            AddEvent(new CartItemAddedEvent(Id, variantId, sku, quantity));
        }

        RecalculateTotals();
        UpdateActivity();
    }

    public void UpdateItemQuantity(Guid variantId, int newQuantity)
    {
        EnsureActive();

        var item = _items.FirstOrDefault(i => i.VariantId == variantId)
            ?? throw new DomainException("Item not found in cart");

        if (newQuantity <= 0)
        {
            RemoveItem(variantId);
            return;
        }

        item.SetQuantity(newQuantity);

        RecalculateTotals();
        UpdateActivity();

        AddEvent(new CartItemQuantityUpdatedEvent(Id, variantId, newQuantity));
    }

    public void RemoveItem(Guid variantId)
    {
        EnsureActive();

        var item = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (item == null) return;

        _items.Remove(item);

        // Clear coupon if cart is empty
        if (!_items.Any())
        {
            ClearCoupon();
        }

        RecalculateTotals();
        UpdateActivity();

        AddEvent(new CartItemRemovedEvent(Id, variantId));
    }

    public void Clear()
    {
        EnsureActive();

        _items.Clear();
        ClearCoupon();
        RecalculateTotals();
        UpdateActivity();

        AddEvent(new CartClearedEvent(Id));
    }

    /// <summary>
    /// Apply discount to a cart item. Called by Pricing service.
    /// </summary>
    public void ApplyItemDiscount(Guid variantId, Money discountPerUnit, string? description, Guid? promotionId = null)
    {
        var item = _items.FirstOrDefault(i => i.VariantId == variantId)
            ?? throw new DomainException("Item not found in cart");

        item.ApplyDiscount(discountPerUnit, description, promotionId);
        RecalculateTotals();
    }

    /// <summary>
    /// Clear all item discounts
    /// </summary>
    public void ClearAllItemDiscounts()
    {
        foreach (var item in _items)
        {
            item.ClearDiscount();
        }
        RecalculateTotals();
    }

    #endregion

    #region Coupon

    public void ApplyCoupon(string couponCode, Guid couponId)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(couponCode))
            throw new DomainException("Coupon code cannot be empty");

        if (!_items.Any())
            throw new DomainException("Cannot apply coupon to empty cart");

        AppliedCouponCode = couponCode.ToUpperInvariant();
        AppliedCouponId = couponId;
        UpdateActivity();

        AddEvent(new CouponAppliedToCartEvent(Id, couponCode, couponId));
    }

    public void RemoveCoupon()
    {
        EnsureActive();
        ClearCoupon();
        UpdateActivity();
    }

    private void ClearCoupon()
    {
        if (AppliedCouponCode != null)
        {
            var oldCode = AppliedCouponCode;
            AppliedCouponCode = null;
            AppliedCouponId = null;

            AddEvent(new CouponRemovedFromCartEvent(Id, oldCode));
        }
    }

    #endregion

    #region Pricing (Called by Application Layer)

    /// <summary>
    /// Update calculated totals. Called by application layer after pricing calculation.
    /// </summary>
    public void UpdateTotals(Money subTotal, Money totalDiscount, Money estimatedTotal)
    {
        SubTotal = subTotal ?? throw new DomainException("SubTotal cannot be null");
        TotalDiscount = totalDiscount ?? new Money(0);
        EstimatedTotal = estimatedTotal ?? throw new DomainException("EstimatedTotal cannot be null");

        IncreaseVersion();
    }

    #endregion

    #region Checkout

    /// <summary>
    /// Mark cart as checked out after Order is created
    /// </summary>
    public void MarkAsCheckedOut(Guid orderId)
    {
        EnsureActive();

        if (!_items.Any())
            throw new DomainException("Cannot checkout empty cart");

        Status = CartStatus.CheckedOut;

        AddEvent(new CartCheckedOutEvent(Id, orderId, Customer.CustomerId, Customer.GuestId));
        IncreaseVersion();
    }

    #endregion

    #region Cart Merge (Guest to Registered)

    /// <summary>
    /// Merge items from another cart (typically guest cart into registered cart)
    /// </summary>
    public void MergeFrom(Cart sourceCart)
    {
        EnsureActive();

        if (sourceCart.Status != CartStatus.Active)
            throw new DomainException("Can only merge from active cart");

        foreach (var item in sourceCart.Items)
        {
            AddItem(
                item.ProductId,
                item.VariantId,
                item.Sku,
                item.ProductName,
                item.VariantName,
                item.Thumbnail,
                item.UnitPrice,
                item.Quantity);
        }

        // Take coupon from source if current doesn't have one
        if (AppliedCouponCode == null && sourceCart.AppliedCouponCode != null)
        {
            AppliedCouponCode = sourceCart.AppliedCouponCode;
            AppliedCouponId = sourceCart.AppliedCouponId;
        }

        AddEvent(new CartMergedEvent(Id, sourceCart.Id));
    }

    /// <summary>
    /// Mark this cart as merged (source cart in merge operation)
    /// </summary>
    public void MarkAsMerged(Guid targetCartId)
    {
        if (Status != CartStatus.Active)
            throw new DomainException("Can only mark active cart as merged");

        Status = CartStatus.Merged;
        IncreaseVersion();
    }

    #endregion

    #region Abandonment

    public void MarkAsAbandoned()
    {
        if (Status != CartStatus.Active)
            return;

        Status = CartStatus.Abandoned;
        AddEvent(new CartAbandonedEvent(Id, Customer.CustomerId, Customer.GuestId));
        IncreaseVersion();
    }

    public void Reactivate()
    {
        if (Status != CartStatus.Abandoned)
            throw new DomainException("Can only reactivate abandoned cart");

        Status = CartStatus.Active;
        UpdateActivity();
        IncreaseVersion();
    }

    #endregion

    #region Private Methods

    private void EnsureActive()
    {
        if (Status != CartStatus.Active)
            throw new DomainException($"Cart is not active (Status: {Status})");
    }

    private void RecalculateTotals()
    {
        var currency = _items.FirstOrDefault()?.UnitPrice.Currency ?? "VND";
        SubTotal = new Money(_items.Sum(i => i.LineTotal.Amount), currency);
        TotalDiscount = new Money(_items.Sum(i => i.LineTotalDiscount.Amount), currency);

        var estimatedTotal = SubTotal.Amount - TotalDiscount.Amount;
        EstimatedTotal = new Money(Math.Max(0, estimatedTotal), currency);
    }

    private void UpdateActivity()
    {
        LastActivityAt = DateTimeOffset.UtcNow;
        IncreaseVersion();
    }

    #endregion
}
