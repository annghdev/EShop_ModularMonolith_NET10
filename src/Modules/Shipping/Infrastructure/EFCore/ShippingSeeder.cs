using Shipping.Domain;

namespace Shipping.Infrastructure;

public class ShippingSeeder
{
    private readonly ShippingDbContext _context;

    public ShippingSeeder(ShippingDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedCarriersAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedCarriersAsync()
    {
        if (_context.ShippingCarriers.Any()) return;

        var carriers = new List<ShippingCarrier>
        {
            ShippingCarrier.Create(
                name: "ghn",
                provider: ShippingProvider.GHN,
                displayName: "Giao Hàng Nhanh",
                description: "Đối tác GHN",
                displayOrder: 1),

            ShippingCarrier.Create(
                name: "ghtk",
                provider: ShippingProvider.GHTK,
                displayName: "Giao Hàng Tiết Kiệm",
                description: "Đối tác GHTK",
                displayOrder: 2),

            ShippingCarrier.Create(
                name: "viettel_post",
                provider: ShippingProvider.ViettelPost,
                displayName: "Viettel Post",
                description: "Đối tác Viettel Post",
                displayOrder: 3),

            ShippingCarrier.Create(
                name: "jt",
                provider: ShippingProvider.JT,
                displayName: "J&T Express",
                description: "Đối tác J&T Express",
                displayOrder: 4),

            ShippingCarrier.Create(
                name: "ninja_van",
                provider: ShippingProvider.NinjaVan,
                displayName: "Ninja Van",
                description: "Đối tác Ninja Van",
                displayOrder: 5),

            ShippingCarrier.Create(
                name: "manual",
                provider: ShippingProvider.Manual,
                displayName: "Tự vận chuyển",
                description: "Shop tự xử lý giao hàng",
                displayOrder: 6)
        };

        await _context.ShippingCarriers.AddRangeAsync(carriers);
    }
}
