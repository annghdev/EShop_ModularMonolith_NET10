namespace Orders.Domain;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string VariantName { get; private set; } = string.Empty;
    public string? Thumbnail { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Money DiscountAmount { get; private set; }

    public Money LineTotal => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);
    public Money LineTotalAfterDiscount => LineTotal.Subtract(DiscountAmount);

    private OrderItem() { } // EF Core

    internal OrderItem(
        Guid productId,
        Guid variantId,
        string sku,
        string productName,
        string variantName,
        string? thumbnail,
        Money unitPrice,
        int quantity)
    {
        if (productId == Guid.Empty)
            throw new DomainException("Product ID cannot be empty");

        if (variantId == Guid.Empty)
            throw new DomainException("Variant ID cannot be empty");

        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0");

        ProductId = productId;
        VariantId = variantId;
        Sku = sku;
        ProductName = productName;
        VariantName = variantName;
        Thumbnail = thumbnail;
        UnitPrice = unitPrice ?? throw new DomainException("Unit price is required");
        Quantity = quantity;
        DiscountAmount = new Money(0, unitPrice.Currency);
    }

    internal void ApplyDiscount(Money discount)
    {
        if (discount.Amount > LineTotal.Amount)
            throw new DomainException("Discount cannot exceed line total");

        DiscountAmount = discount;
    }

    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than 0");

        Quantity = newQuantity;
    }
}
