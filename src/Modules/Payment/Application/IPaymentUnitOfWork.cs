using Kernel.Application.Interfaces;
using Payment.Domain;

namespace Payment.Application;

public interface IPaymentUnitOfWork : IUnitOfWork
{
    IPaymentGatewayRepository PaymentGateways { get; }
    IPaymentTransactionRepository PaymentTransactions { get; }
}
