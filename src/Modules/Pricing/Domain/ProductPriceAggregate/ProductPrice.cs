namespace Pricing.Domain;

public class ProductPrice : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public Sku Sku { get; private set; }
    public Money CurrentCost { get; private set; }
    public Money CurrentPrice { get; private set; }

    private readonly List<PriceChangeLog> _changeLogs = [];
    public IReadOnlyCollection<PriceChangeLog> ChangeLogs => _changeLogs.AsReadOnly();

    private ProductPrice() { } // EF Core

    public static ProductPrice Create(
        Guid productId,
        Guid? variantId,
        Sku sku,
        Money initialCost,
        Money initialPrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("Product ID cannot be empty");

        if (initialCost == null || initialPrice == null)
            throw new DomainException("Initial cost and price are required");

        if (initialPrice.Amount < initialCost.Amount)
            throw new DomainException("Price cannot be less than cost");

        return new ProductPrice
        {
            ProductId = productId,
            VariantId = variantId,
            Sku = sku ?? throw new DomainException("SKU is required"),
            CurrentCost = initialCost,
            CurrentPrice = initialPrice
        };
    }

    public void UpdateCost(Money newCost, string? reason = null, Guid? changedBy = null)
    {
        if (newCost == null)
            throw new DomainException("Cost cannot be null");

        if (newCost.Amount > CurrentPrice.Amount)
            throw new DomainException("Cost cannot exceed current price");

        if (newCost.Amount == CurrentCost.Amount)
            return;

        var log = new PriceChangeLog(
            Id,
            CurrentCost,
            newCost,
            CurrentPrice,
            CurrentPrice,
            PriceChangeType.CostOnly,
            reason,
            changedBy);

        _changeLogs.Add(log);
        CurrentCost = newCost;

        AddEvent(new ProductCostUpdatedEvent(ProductId, VariantId, Sku.Value, CurrentCost));
        IncreaseVersion();
    }

    public void UpdatePrice(Money newPrice, string? reason = null, Guid? changedBy = null)
    {
        if (newPrice == null)
            throw new DomainException("Price cannot be null");

        if (newPrice.Amount < CurrentCost.Amount)
            throw new DomainException("Price cannot be less than current cost");

        if (newPrice.Amount == CurrentPrice.Amount)
            return;

        var log = new PriceChangeLog(
            Id,
            CurrentCost,
            CurrentCost,
            CurrentPrice,
            newPrice,
            PriceChangeType.PriceOnly,
            reason,
            changedBy);

        _changeLogs.Add(log);
        CurrentPrice = newPrice;

        AddEvent(new ProductPriceUpdatedEvent(ProductId, VariantId, Sku.Value, CurrentPrice));
        IncreaseVersion();
    }

    public void UpdateCostAndPrice(Money newCost, Money newPrice, string? reason = null, Guid? changedBy = null)
    {
        if (newCost == null || newPrice == null)
            throw new DomainException("Cost and price cannot be null");

        if (newPrice.Amount < newCost.Amount)
            throw new DomainException("Price cannot be less than cost");

        if (newCost.Amount == CurrentCost.Amount && newPrice.Amount == CurrentPrice.Amount)
            return;

        var log = new PriceChangeLog(
            Id,
            CurrentCost,
            newCost,
            CurrentPrice,
            newPrice,
            PriceChangeType.Both,
            reason,
            changedBy);

        _changeLogs.Add(log);
        CurrentCost = newCost;
        CurrentPrice = newPrice;

        AddEvent(new ProductPriceUpdatedEvent(ProductId, VariantId, Sku.Value, CurrentPrice));
        IncreaseVersion();
    }

    public Money GetProfitMargin()
    {
        return CurrentPrice.Subtract(CurrentCost);
    }

    public decimal GetProfitMarginPercentage()
    {
        if (CurrentPrice.Amount == 0)
            return 0;

        return ((CurrentPrice.Amount - CurrentCost.Amount) / CurrentPrice.Amount) * 100;
    }
}
