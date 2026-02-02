namespace Orders.Domain;

public class OrderDiscount : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }
    public DiscountSourceType Source { get; private set; }
    public Guid? SourceId { get; private set; }
    public string? SourceCode { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Money Amount { get; private set; }

    private OrderDiscount() { } // EF Core

    internal OrderDiscount(
        DiscountSourceType source,
        Guid? sourceId,
        string? sourceCode,
        string description,
        Money amount)
    {
        if (amount == null || amount.Amount < 0)
            throw new DomainException("Discount amount must be non-negative");

        Source = source;
        SourceId = sourceId;
        SourceCode = sourceCode;
        Description = description ?? string.Empty;
        Amount = amount;
    }

    public static OrderDiscount FromCoupon(Guid couponId, string couponCode, string description, Money amount)
    {
        return new OrderDiscount(DiscountSourceType.Coupon, couponId, couponCode, description, amount);
    }

    public static OrderDiscount FromPromotion(Guid promotionId, string description, Money amount)
    {
        return new OrderDiscount(DiscountSourceType.Promotion, promotionId, null, description, amount);
    }

    public static OrderDiscount Manual(string description, Money amount)
    {
        return new OrderDiscount(DiscountSourceType.Manual, null, null, description, amount);
    }
}
