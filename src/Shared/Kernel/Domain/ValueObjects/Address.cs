namespace Kernel.Domain;

public class Address : BaseValueObject
{
    public string RecipientName { get; }
    public string Phone { get; }
    public string Street { get; }
    public string Ward { get; }
    public string District { get; }
    public string City { get; }
    public string? Country { get; }
    public string? PostalCode { get; }

    private Address() { } // EF Core

    public Address(
        string recipientName,
        string phone,
        string street,
        string ward,
        string district,
        string city,
        string? country = "Vietnam",
        string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new DomainException("Recipient name is required");

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Phone is required");

        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street address is required");

        if (string.IsNullOrWhiteSpace(district))
            throw new DomainException("District is required");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City is required");

        RecipientName = recipientName;
        Phone = phone;
        Street = street;
        Ward = ward ?? string.Empty;
        District = district;
        City = city;
        Country = country;
        PostalCode = postalCode;
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { Street };

        if (!string.IsNullOrWhiteSpace(Ward))
            parts.Add(Ward);

        parts.Add(District);
        parts.Add(City);

        if (!string.IsNullOrWhiteSpace(Country))
            parts.Add(Country);

        return string.Join(", ", parts);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RecipientName;
        yield return Phone;
        yield return Street;
        yield return Ward;
        yield return District;
        yield return City;
        yield return Country;
        yield return PostalCode;
    }

    public override string ToString() => $"{RecipientName}, {Phone}, {GetFullAddress()}";
}
