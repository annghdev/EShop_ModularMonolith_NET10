namespace Shipping.Domain;

public class ShipmentItem : BaseEntity
{
    public Guid ShipmentId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string? VariantName { get; private set; }
    public int Quantity { get; private set; }

    private ShipmentItem() { } // EF Core

    public ShipmentItem(
        Guid productId,
        Guid variantId,
        string sku,
        string productName,
        string? variantName,
        int quantity)
    {
        if (productId == Guid.Empty)
            throw new DomainException("Product ID is required");

        if (variantId == Guid.Empty)
            throw new DomainException("Variant ID is required");

        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        ProductId = productId;
        VariantId = variantId;
        Sku = sku;
        ProductName = productName;
        VariantName = variantName;
        Quantity = quantity;
    }
}
