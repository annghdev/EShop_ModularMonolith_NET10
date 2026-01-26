using Payment.Application;
using Payment.Domain;
using Payment.Infrastructure.EFCore.Repositories;

namespace Payment.Infrastructure;

public sealed class PaymentUnitOfWork(PaymentDbContext context, IUserContext user, IPublisher publisher)
    : BaseUnitOfWork<PaymentDbContext>(context, user, publisher), IPaymentUnitOfWork
{
    private IPaymentGatewayRepository? _paymentGatewayRepository;
    private IPaymentTransactionRepository? _paymentTransactionRepository;

    public IPaymentGatewayRepository PaymentGateways =>
        _paymentGatewayRepository ??= new PaymentGatewayRepository(context);

    public IPaymentTransactionRepository PaymentTransactions =>
        _paymentTransactionRepository ??= new PaymentTransactionRepository(context);
}
