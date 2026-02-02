namespace Payment.Domain;

public interface IPaymentGatewayRepository : IRepository<PaymentGateway>
{
    Task<PaymentGateway?> GetByProviderAsync(PaymentProvider provider, CancellationToken cancellationToken = default);
    Task<List<PaymentGateway>> GetActiveGatewaysAsync(CancellationToken cancellationToken = default);
    Task<PaymentGateway?> GetByIdWithConfigurationAsync(Guid id, CancellationToken cancellationToken = default);
}
