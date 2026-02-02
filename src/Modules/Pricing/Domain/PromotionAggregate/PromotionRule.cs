namespace Pricing.Domain;

public class PromotionRule : BaseEntity
{
    public Guid PromotionId { get; private set; }
    public Promotion? Promotion { get; private set; }
    public RuleType Type { get; private set; }
    public Guid? TargetId { get; private set; }
    public int? MinQuantity { get; private set; }
    public Money? MinOrderValue { get; private set; }
    public string? CustomerTier { get; private set; }

    private PromotionRule() { } // EF Core

    internal PromotionRule(
        RuleType type,
        Guid? targetId = null,
        int? minQuantity = null,
        Money? minOrderValue = null,
        string? customerTier = null)
    {
        ValidateRule(type, targetId, minQuantity, minOrderValue, customerTier);

        Type = type;
        TargetId = targetId;
        MinQuantity = minQuantity;
        MinOrderValue = minOrderValue;
        CustomerTier = customerTier;
    }

    private void ValidateRule(
        RuleType type,
        Guid? targetId,
        int? minQuantity,
        Money? minOrderValue,
        string? customerTier)
    {
        switch (type)
        {
            case RuleType.CategoryMatch:
            case RuleType.BrandMatch:
            case RuleType.CollectionMatch:
            case RuleType.ProductMatch:
                if (targetId == null || targetId == Guid.Empty)
                    throw new DomainException($"{type} requires a valid target ID");
                break;
            case RuleType.MinQuantity:
                if (minQuantity == null || minQuantity <= 0)
                    throw new DomainException("Min quantity must be greater than 0");
                break;
            case RuleType.MinOrderValue:
                if (minOrderValue == null)
                    throw new DomainException("Min order value is required");
                break;
            case RuleType.CustomerTier:
                if (string.IsNullOrWhiteSpace(customerTier))
                    throw new DomainException("Customer tier is required");
                break;
        }
    }
}
