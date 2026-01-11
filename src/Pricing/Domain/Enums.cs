namespace Pricing.Domain;

public enum PriceChangeType
{
    CostOnly,
    PriceOnly,
    Both
}

public enum DiscountType
{
    Percentage,
    FixedAmount
}

public enum CouponStatus
{
    Draft,
    Active,
    Expired,
    Disabled
}

public enum CouponConditionType
{
    CategoryRequired,
    BrandRequired,
    CollectionRequired,
    ProductRequired,
    MinQuantity
}

public enum PromotionType
{
    BuyXGetY,
    DiscountByCategory,
    DiscountByBrand,
    DiscountByCollection,
    LoyaltyDiscount,
    BundleDiscount
}

public enum PromotionStatus
{
    Draft,
    Active,
    Ended,
    Disabled
}

public enum RuleType
{
    CategoryMatch,
    BrandMatch,
    CollectionMatch,
    ProductMatch,
    MinQuantity,
    MinOrderValue,
    CustomerTier
}

public enum ActionType
{
    PercentageDiscount,
    FixedAmountDiscount,
    GiftProduct,
    FreeShipping
}
