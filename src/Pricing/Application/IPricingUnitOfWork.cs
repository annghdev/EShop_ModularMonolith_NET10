using Kernel.Application.Interfaces;
using Pricing.Domain;

namespace Pricing.Application;

public interface IPricingUnitOfWork : IUnitOfWork
{
    IProductPriceRepository ProductPrices { get; }
    IPromotionRepository Promotions { get; }
    ICouponRepository Coupons { get; }
}
