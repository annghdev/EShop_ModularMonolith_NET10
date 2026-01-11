namespace Pricing.Domain;

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Coupon? Coupon { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid OrderId { get; private set; }
    public Money DiscountApplied { get; private set; }
    public DateTimeOffset UsedAt { get; private set; }

    private CouponUsage() { } // EF Core

    internal CouponUsage(Guid couponId, Guid customerId, Guid orderId, Money discountApplied)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty");

        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty");

        CouponId = couponId;
        CustomerId = customerId;
        OrderId = orderId;
        DiscountApplied = discountApplied ?? throw new DomainException("Discount applied cannot be null");
        UsedAt = DateTimeOffset.UtcNow;
    }
}
