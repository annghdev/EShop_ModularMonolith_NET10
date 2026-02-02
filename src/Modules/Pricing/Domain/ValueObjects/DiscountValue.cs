namespace Pricing.Domain;

public class DiscountValue : BaseValueObject
{
    public DiscountType Type { get; }
    public decimal Amount { get; }

    private DiscountValue() { } // EF Core

    public DiscountValue(DiscountType type, decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Discount amount cannot be negative");

        if (type == DiscountType.Percentage && amount > 100)
            throw new DomainException("Percentage discount cannot exceed 100%");

        Type = type;
        Amount = amount;
    }

    public Money CalculateDiscount(Money originalPrice)
    {
        var discountAmount = Type switch
        {
            DiscountType.Percentage => originalPrice.Amount * (Amount / 100),
            DiscountType.FixedAmount => Amount,
            _ => 0
        };

        // Cannot discount more than original price
        discountAmount = Math.Min(discountAmount, originalPrice.Amount);

        return new Money(discountAmount, originalPrice.Currency);
    }

    public Money ApplyDiscount(Money originalPrice)
    {
        var discount = CalculateDiscount(originalPrice);
        return originalPrice.Subtract(discount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Amount;
    }

    public override string ToString() =>
        Type == DiscountType.Percentage ? $"{Amount}%" : $"{Amount}";
}
