using Pricing.Application;
using Pricing.Domain;
using Pricing.Infrastructure.EFCore.Repositories;

namespace Pricing.Infrastructure;

public class PricingUnitOfWork(PricingDbContext context, ICurrentUser user, IPublisher publisher)
    : BaseUnitOfWork<PricingDbContext>(context, user, publisher), IPricingUnitOfWork
{
    private IProductPriceRepository? _productPrices;
    private IPromotionRepository? _promotions;
    private ICouponRepository? _coupons;

    public IProductPriceRepository ProductPrices => _productPrices ??= new ProductPriceRepository(context);
    public IPromotionRepository Promotions => _promotions ??= new PromotionRepository(context);
    public ICouponRepository Coupons => _coupons ??= new CouponRepository(context);
}
