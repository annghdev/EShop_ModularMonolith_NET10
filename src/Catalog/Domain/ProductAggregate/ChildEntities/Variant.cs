namespace Catalog.Domain;

public class Variant : AuditableEntity
{
    public string Name { get; private set; }
    public Sku Sku { get; private set; }
    public Money? OverrideCost { get; private set; }
    public Money? OverridePrice { get; private set; }
    public Dimensions? OverrideDimensions { get; private set; }
    public ImageUrl? MainImage { get; private set; }
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }

    private readonly List<ImageUrl> _images = [];
    public IReadOnlyList<ImageUrl> Images => _images.AsReadOnly();

    private readonly List<VariantAttributeValue> _attributeValues = [];
    public IReadOnlyList<VariantAttributeValue> AttributeValues => _attributeValues.AsReadOnly();


    private Variant() { } // EF Core

    public Variant(
        string name,
        Sku sku,
        Money? overrideCost,
        Money? overridePrice,
        ImageUrl? mainImage,
        Dimensions? overrideDimension,
        IEnumerable<(ProductAttribute, Guid)> attributeValues)
    {
        Name = name;
        Sku = sku ?? throw new DomainException("sku cannot be null");
        OverrideCost = overrideCost;
        OverridePrice = overridePrice;
        MainImage = mainImage;
        OverrideDimensions = overrideDimension;

        foreach (var (attr, val) in attributeValues)
        {
            _attributeValues.Add(new VariantAttributeValue
            {
                ProductAttribute = attr,
                ValueId = val
            });
        }

        ValidateAttributeValues();
    }

    private void ValidateAttributeValues()
    {
        if (_attributeValues.Count == 0)
            throw new DomainException("Variant must have at least one attribute value");
    }

    public void UpdatePricing(Money overrideCost, Money overridePrice)
    {
        OverrideCost = overrideCost ?? throw new MoneyException("override cost cannot be null");
        OverridePrice = overridePrice ?? throw new MoneyException("ovrride price cannot be null");
    }

    public void SetMainImage(ImageUrl imageUrl, bool raiseEvent = true)
    {
        if (imageUrl is null)
            throw new DomainException("Main image URL cannot be empty");

        MainImage = imageUrl;

        if (raiseEvent)
        {
            //
        }
    }

    public void AddImage(ImageUrl imageUrl, bool raiseEvent = true)
    {
        if (imageUrl is null)
            throw new DomainException("Main image URL cannot be empty");

        _images.Add(imageUrl);

        if (raiseEvent)
        {
            //
        }
    }

    public Money GetPrice(Money basePrice)
    {
        return OverridePrice ?? Product?.Price
            ?? throw new MoneyException("Product wasnot loaded");
    }

    public Money GetCost(Money basePrice)
    {
        return OverrideCost ?? Product?.Cost
            ?? throw new MoneyException("Product wasnot loaded");
    }
}
