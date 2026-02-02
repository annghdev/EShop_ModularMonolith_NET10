namespace Shipping.Domain;

public class PackageInfo : BaseValueObject
{
    public decimal WeightInGrams { get; }
    public decimal LengthInCm { get; }
    public decimal WidthInCm { get; }
    public decimal HeightInCm { get; }
    public int ItemCount { get; }

    private PackageInfo() { } // EF Core

    public PackageInfo(
        decimal weightInGrams,
        decimal lengthInCm,
        decimal widthInCm,
        decimal heightInCm,
        int itemCount)
    {
        if (weightInGrams < 0)
            throw new DomainException("Weight cannot be negative");

        if (lengthInCm < 0 || widthInCm < 0 || heightInCm < 0)
            throw new DomainException("Dimensions cannot be negative");

        if (itemCount < 0)
            throw new DomainException("Item count cannot be negative");

        WeightInGrams = weightInGrams;
        LengthInCm = lengthInCm;
        WidthInCm = widthInCm;
        HeightInCm = heightInCm;
        ItemCount = itemCount;
    }

    public decimal GetVolumeInCm3() => LengthInCm * WidthInCm * HeightInCm;

    public decimal GetVolumetricWeight(int divisor = 5000)
    {
        // Common formula: (L x W x H) / divisor
        var volume = GetVolumeInCm3();
        return volume / divisor;
    }

    public decimal GetChargeableWeight(int divisor = 5000)
    {
        var volumetricWeight = GetVolumetricWeight(divisor);
        return Math.Max(WeightInGrams / 1000, volumetricWeight);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return WeightInGrams;
        yield return LengthInCm;
        yield return WidthInCm;
        yield return HeightInCm;
        yield return ItemCount;
    }

    public static PackageInfo Default() => new(0, 0, 0, 0, 0);
}
