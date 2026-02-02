namespace Pricing.Domain;

public class PromotionAction : BaseEntity
{
    public Guid PromotionId { get; private set; }
    public Promotion? Promotion { get; private set; }
    public ActionType Type { get; private set; }
    public DiscountValue? Discount { get; private set; }
    public Guid? GiftProductId { get; private set; }
    public Guid? GiftVariantId { get; private set; }
    public int GiftQuantity { get; private set; }

    private PromotionAction() { } // EF Core

    internal PromotionAction(
        ActionType type,
        DiscountValue? discount = null,
        Guid? giftProductId = null,
        Guid? giftVariantId = null,
        int giftQuantity = 1)
    {
        ValidateAction(type, discount, giftProductId);

        Type = type;
        Discount = discount;
        GiftProductId = giftProductId;
        GiftVariantId = giftVariantId;
        GiftQuantity = giftQuantity;
    }

    private void ValidateAction(ActionType type, DiscountValue? discount, Guid? giftProductId)
    {
        switch (type)
        {
            case ActionType.PercentageDiscount:
            case ActionType.FixedAmountDiscount:
                if (discount == null)
                    throw new DomainException("Discount value is required for discount actions");
                break;
            case ActionType.GiftProduct:
                if (giftProductId == null || giftProductId == Guid.Empty)
                    throw new DomainException("Gift product ID is required");
                break;
            case ActionType.FreeShipping:
                // No additional validation needed
                break;
        }
    }

    public Money? CalculateDiscount(Money originalPrice)
    {
        return Type switch
        {
            ActionType.PercentageDiscount => Discount?.CalculateDiscount(originalPrice),
            ActionType.FixedAmountDiscount => Discount?.CalculateDiscount(originalPrice),
            _ => null
        };
    }
}
