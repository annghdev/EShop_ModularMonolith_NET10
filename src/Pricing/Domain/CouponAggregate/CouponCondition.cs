namespace Pricing.Domain;

public class CouponCondition : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Coupon? Coupon { get; private set; }
    public CouponConditionType Type { get; private set; }
    public Guid? TargetId { get; private set; }
    public int? MinQuantity { get; private set; }

    private CouponCondition() { } // EF Core

    internal CouponCondition(CouponConditionType type, Guid? targetId, int? minQuantity)
    {
        ValidateCondition(type, targetId, minQuantity);

        Type = type;
        TargetId = targetId;
        MinQuantity = minQuantity;
    }

    private void ValidateCondition(CouponConditionType type, Guid? targetId, int? minQuantity)
    {
        switch (type)
        {
            case CouponConditionType.CategoryRequired:
            case CouponConditionType.BrandRequired:
            case CouponConditionType.CollectionRequired:
            case CouponConditionType.ProductRequired:
                if (targetId == null || targetId == Guid.Empty)
                    throw new DomainException($"{type} requires a valid target ID");
                break;
            case CouponConditionType.MinQuantity:
                if (minQuantity == null || minQuantity <= 0)
                    throw new DomainException("Min quantity must be greater than 0");
                break;
        }
    }
}
