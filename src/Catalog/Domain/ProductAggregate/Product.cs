namespace Catalog.Domain;

public class Product : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Slug Slug { get; private set; }
    public Money Cost { get; private set; } = new(0);
    public Money Price { get; private set; }
    public Dimensions Dimensions { get; private set; } = new(1, 1, 1, 1);
    public ImageUrl? Thumbnail { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public Guid BrandId { get; private set; }
    public Brand? Brand { get; private set; }
    public bool HastockQuantity { get; private set; } = true;
    public int DisplayPriority { get; private set; } = 1; // 0 = Is Featured
    public ProductStatus Status { get; private set; }

    private readonly List<ImageUrl> _images = [];
    public IReadOnlyList<ImageUrl> Images => _images.AsReadOnly();

    private readonly List<ProductAttribute> _attributes = [];
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

    private readonly List<Variant> _variants = [];
    public IReadOnlyCollection<Variant> Variants => _variants.AsReadOnly();


    private Product() { } // EF Core

    public static Product CreateDraft(
        Guid Id, string name, string? description, Sku sku,
        Money cost, Money price, Dimensions dimensions, bool hasStockQuantity,
        Category category, Guid brandId)
    {
        var draft = new Product
        {
            Id = Id,
            Name = name ?? throw new DomainException("Name cannot be null"),
            Description = description,
            Cost = cost ?? throw new DomainException("Cost cannot be null"),
            Price = price ?? throw new DomainException("Price cannot be null"),
            Dimensions = dimensions ?? throw new DomainException("Dimensions cannot be null"),
            HastockQuantity = hasStockQuantity,
            Category = category,
            BrandId = brandId,
            Slug = new(name),
            Status = ProductStatus.Draft,
        };

        return draft;
    }

    public void UpdateBasicInfo(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required");

        Name = name;
        Description = description;
        Slug = new(name);

        AddEvent(new ProductBasicInfoUpdatedEvent(this));

        IncreaseVersion();
    }

    public void UpdatePricing(Money cost, Money price)
    {
        if (price.Amount < cost.Amount)
            throw new DomainException("Price cannot be less than cost");

        var oldPrice = Price.Amount;
        Cost = cost;
        Price = price;

        if (oldPrice != price.Amount)
        {
            AddEvent(new ProductPriceUpdatedEvent(Id, Cost.Amount, Price.Amount));
        }

        IncreaseVersion();
    }

    public void UpdateDimensions(Dimensions dimensions)
    {
        Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));

        // raise event here

        IncreaseVersion();
    }

    public void SetCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException("Category ID cannot be empty");

        CategoryId = categoryId;
        IncreaseVersion();
    }

    public void Publish()
    {
        if (Status == ProductStatus.Discarded)
            throw new DomainException("Draft was discarded");

        ValidateProduct();

        Status = ProductStatus.Published;

        AddEvent(new ProductPublishedEvent(this));
        IncreaseVersion();
    }

    public void Discontinue()
    {
        if (Status == ProductStatus.Discarded) return;


        Status = ProductStatus.Discontinued;
        AddEvent(new ProductDiscontinuedEvent(Id));
        IncreaseVersion();
    }

    public void UpdateThumbnail(ImageUrl imageUrl, bool raiseEvent = true)
    {
        Thumbnail = imageUrl;
        AddEvent(new ProductThumbnailUpdatedEvent(Id, imageUrl.Path));
        IncreaseVersion();
    }

    public void AddImage(ImageUrl imageUrl)
    {
        if (_images.Any(i => i == imageUrl))
            throw new DomainException("Image already exists");

        _images.Add(imageUrl);
        AddEvent(new ProductImageAddedEvent(Id, imageUrl.Path));
        IncreaseVersion();
    }

    public void RemoveImage(ImageUrl imageUrl)
    {
        if (!_images.Any(i => i == imageUrl))
            throw new DomainException("Image not exists");
        _images.Remove(imageUrl);
        IncreaseVersion();
    }

    public void AddAttribute(Guid attributeId, Guid defaultValueId, int displayOrder, bool raiseEvent = true)
    {
        if (_attributes.Any(a => a.AttributeId == attributeId))
            throw new DomainException("Attribute already exists in product");

        var productAttribute = new ProductAttribute(attributeId, displayOrder);
        _attributes.Add(productAttribute);

        if(raiseEvent)
        {
            // add event here
        }

        IncreaseVersion();
    }

    public void RemoveAttribute(Guid attributeId, bool raiseEvent = true)
    {
        var attribute = _attributes.FirstOrDefault(a => a.AttributeId == attributeId);
        if (attribute == null)
            throw new DomainException("Attribute not found");

        _attributes.Remove(attribute);

        if (raiseEvent)
        {
            // add event here
        }

        IncreaseVersion();
    }

    public void AddVariant(Variant variant, bool raiseEvent = true)
    {
        if (variant == null)
            throw new ArgumentNullException(nameof(variant));

        if (_variants.Any(v => v.Sku.Value == variant.Sku.Value))
            throw new DomainException($"Variant with SKU {variant.Sku.Value} already exists");

        ValidateVariantAttributes(variant);

        _variants.Add(variant);
        if (raiseEvent)
            AddEvent(new VariantAddedEvent(variant));
        IncreaseVersion();
    }

    private void ValidateVariantAttributes(Variant variant)
    {
        foreach (var attrValue in variant.AttributeValues)
        {
            if (!_attributes.Any(a => a.Id == attrValue.ProductAttributeId))
                throw new DomainException("Variant contains attribute not defined in product");
        }
    }

    public void ValidateVariants()
    {
        foreach (var variant in Variants)
        {
            ValidateVariantAttributes(variant);
        }
    }

    public void ValidateCategoryDefaultAttributes()
    {
        foreach (var defaultAttr in Category!.GetAllDefaultAttributesFromHierarchy())
        {
            if (!_attributes.Any(a => a.AttributeId == defaultAttr.AttributeId))
                throw new DomainException($"Product missing required attribute '{defaultAttr.Attribute.Name}' from category hierarchy");
        }
    }

    public void RemoveVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
            throw new DomainException("Variant not found");

        _variants.Remove(variant);

        // add event here

        IncreaseVersion();
    }

    private void ValidateProduct()
    {
        if (!_variants.Any())
            throw new DomainException("Product must has at least 1 variant");
    }

    public void UpdateDisplayPriority(int priority)
    {
        if (priority < 0)
            throw new DomainException("Display priority cannot be negative");

        DisplayPriority = priority;

        // add event here

        IncreaseVersion();
    }

    public void Discard()
    {
        if (Status != ProductStatus.Draft)
            throw new DomainException("Cannot discard product after publish");

        Status = ProductStatus.Discarded;

        // add event here
    }
}

public enum ProductStatus
{
    Draft,
    Discarded,
    Published,
    Discontinued
}
