namespace Kernel.Domain;

public class Sku : BaseValueObject
{
    public string Value { get; }

    private Sku() { }

    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("SKU cannot be empty");

        if (value.Length < 3 || value.Length > 50)
            throw new DomainException("SKU must be between 3 and 50 characters");

        Value = value.ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}