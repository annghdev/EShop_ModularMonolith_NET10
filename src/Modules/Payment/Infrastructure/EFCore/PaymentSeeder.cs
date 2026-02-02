using Payment.Domain;

namespace Payment.Infrastructure;

public class PaymentSeeder
{
    private readonly PaymentDbContext _context;

    public PaymentSeeder(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedPaymentGatewaysAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedPaymentGatewaysAsync()
    {
        if (_context.PaymentGateways.Any()) return;

        var gateways = new List<PaymentGateway>();

        // COD - Cash on Delivery (enabled by default)
        var codGateway = PaymentGateway.Create(
            name: "cod",
            provider: PaymentProvider.COD,
            displayName: "Thanh toán khi nhận hàng (COD)",
            description: "Thanh toán trực tiếp cho người giao hàng khi nhận được đơn hàng",
            displayOrder: 1,
            supportedCurrencies: ["VND"]);
        codGateway.Enable();
        gateways.Add(codGateway);

        // VNPay
        var vnpayGateway = PaymentGateway.Create(
            name: "vnpay",
            provider: PaymentProvider.VNPay,
            displayName: "VNPay",
            description: "Thanh toán qua cổng VNPay - Hỗ trợ thẻ ATM, Visa, MasterCard",
            displayOrder: 2,
            supportedCurrencies: ["VND"]);
        gateways.Add(vnpayGateway);

        // Momo
        var momoGateway = PaymentGateway.Create(
            name: "momo",
            provider: PaymentProvider.Momo,
            displayName: "Ví MoMo",
            description: "Thanh toán qua ví điện tử MoMo",
            displayOrder: 3,
            supportedCurrencies: ["VND"]);
        gateways.Add(momoGateway);

        // ZaloPay
        var zalopayGateway = PaymentGateway.Create(
            name: "zalopay",
            provider: PaymentProvider.ZaloPay,
            displayName: "ZaloPay",
            description: "Thanh toán qua ví điện tử ZaloPay",
            displayOrder: 4,
            supportedCurrencies: ["VND"]);
        gateways.Add(zalopayGateway);

        // Bank Transfer
        var bankTransferGateway = PaymentGateway.Create(
            name: "bank_transfer",
            provider: PaymentProvider.BankTransfer,
            displayName: "Chuyển khoản ngân hàng",
            description: "Chuyển khoản trực tiếp đến tài khoản ngân hàng của shop",
            displayOrder: 5,
            supportedCurrencies: ["VND"]);
        gateways.Add(bankTransferGateway);

        await _context.PaymentGateways.AddRangeAsync(gateways);
    }
}
