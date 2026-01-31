namespace Catalog.Domain;

public class Attribute : AggregateRoot
{
    public required string Name { get; set; }
    public string? Icon { get; set; }
    public string? ValueStyleCss { get; set; }
    public bool DisplayText { get; set; }
    public ICollection<AttributeValue> Values { get; set; } = [];

    public void UpdateInfo(string name, string? icon, bool displayText, string? valueStyleCss)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Attribute name is required");

        Name = name;
        Icon = icon;
        DisplayText = displayText;
        ValueStyleCss = valueStyleCss;
        IncreaseVersion();
    }

    public AttributeValue AddValue(string name, string? colorCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Attribute value name is required");

        if (Values.Any(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Attribute value '{name}' already exists");

        var value = new AttributeValue
        {
            Name = name,
            ColorCode = colorCode
        };

        Values.Add(value);
        IncreaseVersion();
        return value;
    }

    public void UpdateValue(Guid valueId, string name, string? colorCode)
    {
        var value = Values.FirstOrDefault(v => v.Id == valueId);
        if (value == null)
            throw new DomainException("Attribute value not found");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Attribute value name is required");

        if (Values.Any(v => v.Id != valueId && v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Attribute value '{name}' already exists");

        value.Name = name;
        value.ColorCode = colorCode;
        IncreaseVersion();
    }

    public void RemoveValue(Guid valueId)
    {
        var value = Values.FirstOrDefault(v => v.Id == valueId);
        if (value == null)
            throw new DomainException("Attribute value not found");

        Values.Remove(value);
        IncreaseVersion();
    }
}
