using Pricing.Domain;

namespace Pricing.Infrastructure;

public class PricingSeeder
{
    private readonly PricingDbContext _context;

    public PricingSeeder(PricingDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedCouponsAsync();
        await SeedPromotionsAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedCouponsAsync()
    {
        if (_context.Coupons.Any()) return;

        var now = DateTimeOffset.UtcNow;
        var startDate = now.AddDays(-7);
        var endDate = now.AddDays(30);

        var welcomeCoupon = Coupon.Create(
            "WELCOME10",
            "Welcome 10% Off",
            "Discount 10% for first-time orders",
            new DiscountValue(DiscountType.Percentage, 10),
            startDate,
            endDate,
            minOrderValue: new Money(100_000),
            maxDiscountAmount: new Money(50_000),
            usageLimitTotal: 1000,
            usageLimitPerCustomer: 1);
        welcomeCoupon.Id = Guid.CreateVersion7();
        welcomeCoupon.Activate();

        var saveFixedCoupon = Coupon.Create(
            "SAVE50K",
            "Save 50K",
            "Fixed 50,000 VND discount on orders",
            new DiscountValue(DiscountType.FixedAmount, 50_000),
            startDate,
            endDate,
            minOrderValue: new Money(300_000),
            usageLimitTotal: 500);
        saveFixedCoupon.Id = Guid.CreateVersion7();
        saveFixedCoupon.AddCondition(CouponConditionType.MinQuantity, minQuantity: 2);
        saveFixedCoupon.Activate();

        var weekendCoupon = Coupon.Create(
            "WEEKEND5",
            "Weekend 5% Off",
            "Small discount for weekend orders",
            new DiscountValue(DiscountType.Percentage, 5),
            now.AddDays(-1),
            now.AddDays(7),
            minOrderValue: new Money(200_000));
        weekendCoupon.Id = Guid.CreateVersion7();

        await _context.Coupons.AddRangeAsync(welcomeCoupon, saveFixedCoupon, weekendCoupon);
    }

    private async Task SeedPromotionsAsync()
    {
        if (_context.Promotions.Any()) return;

        var now = DateTimeOffset.UtcNow;
        var startDate = now.AddDays(-3);
        var endDate = now.AddDays(30);

        var loyaltyPromotion = Promotion.Create(
            "Loyalty Discount",
            "5% discount for eligible customers",
            PromotionType.LoyaltyDiscount,
            priority: 1,
            isStackable: true,
            startDate,
            endDate);
        loyaltyPromotion.Id = Guid.CreateVersion7();
        loyaltyPromotion.AddRule(RuleType.MinOrderValue, minOrderValue: new Money(500_000));
        loyaltyPromotion.AddRule(RuleType.CustomerTier, customerTier: "Gold");
        loyaltyPromotion.AddAction(ActionType.PercentageDiscount, new DiscountValue(DiscountType.Percentage, 5));
        loyaltyPromotion.Activate();

        var bundlePromotion = Promotion.Create(
            "Bundle Discount",
            "Fixed discount for bundle orders",
            PromotionType.BundleDiscount,
            priority: 2,
            isStackable: false,
            startDate,
            endDate);
        bundlePromotion.Id = Guid.CreateVersion7();
        bundlePromotion.AddRule(RuleType.MinQuantity, minQuantity: 3);
        bundlePromotion.AddAction(ActionType.FixedAmountDiscount, new DiscountValue(DiscountType.FixedAmount, 30_000));
        bundlePromotion.Activate();

        var freeShippingPromotion = Promotion.Create(
            "Free Shipping",
            "Free shipping for orders over 1,000,000 VND",
            PromotionType.LoyaltyDiscount,
            priority: 3,
            isStackable: true,
            startDate,
            endDate);
        freeShippingPromotion.Id = Guid.CreateVersion7();
        freeShippingPromotion.AddRule(RuleType.MinOrderValue, minOrderValue: new Money(1_000_000));
        freeShippingPromotion.AddAction(ActionType.FreeShipping);
        freeShippingPromotion.Activate();

        await _context.Promotions.AddRangeAsync(loyaltyPromotion, bundlePromotion, freeShippingPromotion);
    }
}
