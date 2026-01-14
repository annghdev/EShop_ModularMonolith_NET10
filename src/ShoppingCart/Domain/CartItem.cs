namespace ShoppingCart.Domain;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Cart? Cart { get; private set; }

    // Product snapshot info
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string VariantName { get; private set; } = string.Empty;
    public string? Thumbnail { get; private set; }

    // Pricing
    public Money OriginalPrice { get; private set; }  // Giá gốc (hiển thị gạch ngang nếu có discount)
    public Money UnitPrice { get; private set; }       // Giá hiện tại sau discount

    // Discount info (applied by Pricing service)
    public Money DiscountAmount { get; private set; }  // Discount trên mỗi item
    public string? DiscountDescription { get; private set; }  // "Giảm 20%", "Flash Sale", etc.
    public Guid? AppliedPromotionId { get; private set; }  // Promotion đã áp dụng (nếu có)

    // Quantity
    public int Quantity { get; private set; }

    // Calculated totals
    public Money LineTotal => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);
    public Money LineTotalBeforeDiscount => new(OriginalPrice.Amount * Quantity, OriginalPrice.Currency);
    public Money LineTotalDiscount => new(DiscountAmount.Amount * Quantity, DiscountAmount.Currency);

    public bool HasDiscount => DiscountAmount.Amount > 0;

    private CartItem() { } // EF Core

    internal CartItem(
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
        OriginalPrice = unitPrice ?? throw new DomainException("Unit price is required");
        UnitPrice = unitPrice;
        DiscountAmount = new Money(0, unitPrice.Currency);
        Quantity = quantity;
    }

    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than 0");

        Quantity += amount;
    }

    internal void SetQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than 0");

        Quantity = newQuantity;
    }

    internal void UpdatePrice(Money newPrice)
    {
        OriginalPrice = newPrice ?? throw new DomainException("Price cannot be null");
        UnitPrice = newPrice;
        ClearDiscount();
    }

    internal void UpdateProductInfo(string productName, string variantName, string? thumbnail)
    {
        ProductName = productName ?? ProductName;
        VariantName = variantName ?? VariantName;
        Thumbnail = thumbnail;
    }

    /// <summary>
    /// Apply discount to this item. Called by Pricing service.
    /// </summary>
    internal void ApplyDiscount(Money discountPerUnit, string? description, Guid? promotionId = null)
    {
        if (discountPerUnit.Amount < 0)
            throw new DomainException("Discount cannot be negative");

        if (discountPerUnit.Amount > OriginalPrice.Amount)
            throw new DomainException("Discount cannot exceed original price");

        DiscountAmount = discountPerUnit;
        DiscountDescription = description;
        AppliedPromotionId = promotionId;
        UnitPrice = OriginalPrice.Subtract(discountPerUnit);
    }

    /// <summary>
    /// Clear any applied discount
    /// </summary>
    internal void ClearDiscount()
    {
        DiscountAmount = new Money(0, OriginalPrice.Currency);
        DiscountDescription = null;
        AppliedPromotionId = null;
        UnitPrice = OriginalPrice;
    }
}
