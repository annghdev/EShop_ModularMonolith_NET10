namespace Payment.Domain;

public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
{
    Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PaymentTransaction?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default);
    Task<PaymentTransaction?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<List<PaymentTransaction>> GetExpiredTransactionsAsync(CancellationToken cancellationToken = default);
    Task<PaymentTransaction?> GetByIdWithRefundsAsync(Guid id, CancellationToken cancellationToken = default);
}
