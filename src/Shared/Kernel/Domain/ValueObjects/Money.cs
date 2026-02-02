namespace Kernel.Domain;

public class Money : BaseValueObject
{
    public decimal Amount { get; }
    public string Currency { get; } = string.Empty;

    private Money() { } // For EF Core

    public Money(decimal amount, string? currency = "VND")
    {
        if (string.IsNullOrEmpty(currency))
            currency = "VND";

        if (amount < 0)
            throw new MoneyException("The amount must not be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new MoneyException("Currency invalid");

        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("It is not possible to add or subtract different currencies.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency}";
}
