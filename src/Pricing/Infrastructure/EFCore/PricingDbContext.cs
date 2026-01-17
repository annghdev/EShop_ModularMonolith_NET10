using System.Reflection;

using Pricing.Domain;

namespace Pricing.Infrastructure;

public class PricingDbContext(DbContextOptions<PricingDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<ProductPrice> ProductPrices { get; set; }
    public DbSet<PriceChangeLog> PriceChangeLogs { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponCondition> CouponConditions { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<PromotionRule> PromotionRules { get; set; }
    public DbSet<PromotionAction> PromotionActions { get; set; }

    protected override Assembly GetAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}
