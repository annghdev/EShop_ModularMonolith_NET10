using Contracts;
using Microsoft.Extensions.Logging;
using Users.Domain;

namespace Users.Application;

/// <summary>
/// Wolverine consumer to create Customer profile after Auth registration.
/// </summary>
public class AccountRegisteredForCustomerEventConsumer
{
    public async Task Handle(
        AccountRegiteredForcustomerIntegrationEvent @event,
        IUsersUnitOfWork uow,
        ILogger<AccountRegisteredForCustomerEventConsumer> logger,
        CancellationToken cancellationToken)
    {
        var existingCustomer = await uow.Customers.GetByAccountIdAsync(@event.AccountId, cancellationToken);
        if (existingCustomer is not null)
        {
            logger.LogInformation("Skip account registration sync because AccountId {AccountId} already exists.", @event.AccountId);
            return;
        }

        var email = new Email(@event.Email);
        var fullName = string.IsNullOrWhiteSpace(@event.FullName) ? @event.Email : @event.FullName.Trim();
        var customer = Customer.Create(fullName, email);

        if (!string.IsNullOrWhiteSpace(@event.GuestId))
        {
            var guest = await uow.Guests.GetByGuestIdAsync(@event.GuestId, cancellationToken);
            if (guest is not null)
            {
                customer = Customer.CreateFromGuest(guest, fullName, email);

                var guestAggregate = await uow.Guests.LoadFullAggregate(guest.Id);
                guestAggregate.MarkAsConverted(customer.Id);
                uow.Guests.Update(guestAggregate);
            }
        }

        customer.LinkAccount(@event.AccountId);

        if (!string.IsNullOrWhiteSpace(@event.PhoneNumber))
        {
            try
            {
                customer.UpdateProfile(fullName, new PhoneNumber(@event.PhoneNumber), customer.DateOfBirth, customer.Gender);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Skip mapping invalid phone number for AccountId {AccountId}.", @event.AccountId);
            }
        }

        uow.Customers.Add(customer);
        await uow.CommitAsync(cancellationToken);
    }
}
