namespace Pricing.Domain;

public class PriceChangeLog : BaseEntity
{
    public Guid ProductPriceId { get; private set; }
    public ProductPrice? ProductPrice { get; private set; }
    public Money PreviousCost { get; private set; }
    public Money NewCost { get; private set; }
    public Money PreviousPrice { get; private set; }
    public Money NewPrice { get; private set; }
    public PriceChangeType ChangeType { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ChangedBy { get; private set; }

    private PriceChangeLog() { } // EF Core

    internal PriceChangeLog(
        Guid productPriceId,
        Money previousCost,
        Money newCost,
        Money previousPrice,
        Money newPrice,
        PriceChangeType changeType,
        string? reason,
        Guid? changedBy)
    {
        ProductPriceId = productPriceId;
        PreviousCost = previousCost;
        NewCost = newCost;
        PreviousPrice = previousPrice;
        NewPrice = newPrice;
        ChangeType = changeType;
        Reason = reason;
        ChangedBy = changedBy;
    }
}
