namespace Kernel.Domain;

public class Dimensions : BaseValueObject
{
    public decimal Width { get; }
    
    public decimal Height { get; }
    
    public decimal Depth { get; }

    public decimal Weight { get; }

    private Dimensions() { } // For EF Core

    public Dimensions(decimal width, decimal height, decimal depth, decimal weight)
    {
        if (width <= 0 || height <= 0 || depth <= 0 || weight <= 0)
            throw new DomainException("Dimensions must be greater than 0");

        Width = width;
        Height = height;
        Depth = depth;
        Weight = weight;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Width;
        yield return Height;
        yield return Depth;
        yield return Weight;
    }

    public override string ToString() => $"{Width}x{Height}x{Depth}, {Weight}kg";
}
