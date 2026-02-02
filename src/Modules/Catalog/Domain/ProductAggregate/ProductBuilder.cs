namespace Catalog.Domain;

public sealed class ProductBuilder
{
    private readonly Product _product;

    private ProductBuilder(Product product)
    {
        _product = product;
    }

    public static ProductBuilder Draft(
        Guid id,
        string name,
        string? description,
        Sku sku,
        Money cost,
        Money price,
        Dimensions dimensions,
        bool hasStockQuantity,
        Category category,
        Guid brandId)
    {
        var product = Product.CreateDraft(
            id, name, description, sku, cost, price, dimensions, hasStockQuantity, category, brandId
        );

        return new ProductBuilder(product);
    }

    public ProductBuilder SetThumbnail(string? thumbnail)
    {
        if (!string.IsNullOrEmpty(thumbnail))
            _product.UpdateThumbnail(new(thumbnail), false);
        return this;
    }

    public ProductBuilder SetImages(IEnumerable<string> images)
    {
        foreach (var image in images)
        {
            _product.AddImage(new(image));
        }
        return this;
    }

    public ProductBuilder AddAttribute(
        Guid attributeId,
        Guid defaultValueId,
        int displayOrder,
        bool hasVariant = false)
    {
        _product.AddAttribute(attributeId, defaultValueId, displayOrder, hasVariant);
        return this;
    }

    public ProductBuilder AddVariant(
        string? name,
        Sku sku,
        Dictionary<Guid, Guid> attributes,
        string? mainImageUrl,
        IEnumerable<string> images,
        Money? overrideCost = null,
        Money? overridePrice = null,
        Dimensions? overrideDimensions = null)
    {
        if (!_product.Attributes.Any())
            throw new DomainException(
                "Must add attributes before adding variants");

        var attrs = new List<(ProductAttribute, Guid)>();
        foreach (var item in _product.Attributes)
        {
            (ProductAttribute, Guid) newItem = new();
            newItem.Item1 = item;
            newItem.Item2 = attributes.FirstOrDefault(a => a.Key == item.AttributeId).Value;

            attrs.Add(newItem);
        }

        ImageUrl? mainImage = null;
        if (!string.IsNullOrEmpty(mainImageUrl))
        {
            mainImage = new ImageUrl(mainImageUrl);
        }

        var variant = new Variant(name, sku, overrideCost, overridePrice, mainImage, overrideDimensions, attrs);

        foreach (var image in images)
        {
            variant.AddImage(new ImageUrl(image), raiseEvent: false);
        }

        _product.AddVariant(variant, raiseEvent: false);
        return this;
    }

    /// <summary>
    /// Build the product with full validation (requires variants).
    /// Use this when publishing a product.
    /// </summary>
    public Product Build()
    {
        if (_product.Category != null && _product.Category.DefaultAttributes.Any() && !_product.Attributes.Any())
            throw new DomainException("Product must have default attributes");

        if (!_product.Variants.Any())
            throw new DomainException("Product must have variants");

        _product.ValidateCategoryDefaultAttributes();
        _product.ValidateVariants();
        return _product;
    }

    /// <summary>
    /// Build as draft without requiring variants.
    /// Variants can be added later before publishing.
    /// </summary>
    public Product BuildDraft()
    {
        // Draft products don't require variants - they can be added later
        // Only validate attributes if there are any
        if (_product.Attributes.Any())
        {
            _product.ValidateCategoryDefaultAttributes();
        }
        
        return _product;
    }
}
