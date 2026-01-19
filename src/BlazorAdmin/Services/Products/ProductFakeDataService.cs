using Contracts.Responses.Catalog;
using Contracts.Requests.Catalog;
using Contracts.Responses;

namespace BlazorAdmin.Services;

public class ProductFakeDataService : IProductService
{
    private readonly List<ProductDto> _products = [];
    private readonly List<CategoryDto> _categories = [];
    private readonly List<BrandDto> _brands = [];
    private readonly List<AttributeDto> _attributes = [];
    
    public ProductFakeDataService()
    {
        SeedFakeData();
    }
    
    private void SeedFakeData()
    {
        // Seed categories
        _categories.AddRange(new[]
        {
            new CategoryDto { Id = Guid.NewGuid(), Name = "Electronics" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Fashion" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Home & Garden" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Sports" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Books" }
        });
        
        // Seed brands
        _brands.AddRange(new[]
        {
            new BrandDto { Id = Guid.NewGuid(), Name = "Samsung", Logo = "https://picsum.photos/seed/samsung/100/100" },
            new BrandDto { Id = Guid.NewGuid(), Name = "Apple", Logo = "https://picsum.photos/seed/apple/100/100" },
            new BrandDto { Id = Guid.NewGuid(), Name = "Nike", Logo = "https://picsum.photos/seed/nike/100/100" },
            new BrandDto { Id = Guid.NewGuid(), Name = "Adidas", Logo = "https://picsum.photos/seed/adidas/100/100" }
        });
        
        // Seed attributes
        var colorAttributeId = Guid.NewGuid();
        var sizeAttributeId = Guid.NewGuid();
        
        _attributes.AddRange(new[]
        {
            new AttributeDto 
            { 
                Id = colorAttributeId, 
                Name = "Color", 
                Icon = "🎨",
                Values = 
                [
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "Red", ColorCode = "#FF0000" },
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "Blue", ColorCode = "#0000FF" },
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "Green", ColorCode = "#00FF00" },
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "Black", ColorCode = "#000000" },
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "White", ColorCode = "#FFFFFF" }
                ]
            },
            new AttributeDto 
            { 
                Id = sizeAttributeId, 
                Name = "Size",
                Icon = "📏",
                Values = 
                [
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "S" },
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "M" },
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "L" },
                    new AttributeValueDto { Id = Guid.NewGuid(), Value = "XL" }
                ]
            }
        });
        
        // Seed products with variants and attributes
        string[] statuses = ["Draft", "Published", "Discontinued"];
        string[] productNames = 
        [
            "Smartphone", "Laptop", "T-Shirt", "Jeans", "Running Shoes",
            "Coffee Maker", "Desk Lamp", "Backpack", "Headphones", "Watch",
            "Tablet", "Camera", "Jacket", "Sneakers", "Sofa",
            "Microwave", "Keyboard", "Mouse", "Monitor", "Chair"
        ];
        
        for (int i = 0; i < productNames.Length; i++)
        {
            var category = _categories[Random.Shared.Next(_categories.Count)];
            var brand = _brands[Random.Shared.Next(_brands.Count)];
            var status = statuses[i % 3];
            
            var colorAttr = _attributes[0];
            var sizeAttr = _attributes[1];
            
            var product = new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = $"{brand.Name} {productNames[i]}",
                Description = $"High quality {productNames[i].ToLower()} from {brand.Name}. Perfect for everyday use with excellent features and durability.",
                Slug = $"{brand.Name.ToLower()}-{productNames[i].ToLower()}-{i}",
                Sku = $"SKU-{i + 1:D4}",
                Cost = new MoneyDto { Amount = 100000 + (i * 10000), Currency = "VND" },
                Price = new MoneyDto { Amount = 150000 + (i * 15000), Currency = "VND" },
                Dimensions = new DimensionsDto { Width = 10 + i, Height = 5 + i, Depth = 3, Weight = 0.5m + (i * 0.1m) },
                HasStockQuantity = true,
                Thumbnail = $"https://picsum.photos/seed/product{i}/400/300",
                Images = 
                [
                    $"https://picsum.photos/seed/product{i}-1/800/600",
                    $"https://picsum.photos/seed/product{i}-2/800/600",
                    $"https://picsum.photos/seed/product{i}-3/800/600"
                ],
                Category = category,
                Brand = brand,
                DisplayPriority = i,
                Status = status,
                Attributes = 
                [
                    new ProductAttributeDto { AttributeId = colorAttributeId, AttributeName = "Color", DisplayOrder = 1, HasVariant = true },
                    new ProductAttributeDto { AttributeId = sizeAttributeId, AttributeName = "Size", DisplayOrder = 2, HasVariant = true }
                ],
                Variants = [],
                CreatedAt = DateTimeOffset.Now.AddDays(-(productNames.Length - i)),
                UpdatedAt = DateTimeOffset.Now.AddDays(-(productNames.Length - i) / 2.0)
            };
            
            // Create 2-4 variants per product
            int variantCount = Random.Shared.Next(2, 5);
            for (int v = 0; v < variantCount; v++)
            {
                var colorValue = colorAttr.Values[Random.Shared.Next(colorAttr.Values.Count)];
                var sizeValue = sizeAttr.Values[Random.Shared.Next(sizeAttr.Values.Count)];
                
                var variant = new VariantDto
                {
                    Id = Guid.NewGuid(),
                    Name = $"{product.Name} - {colorValue.Value}/{sizeValue.Value}",
                    Sku = $"{product.Sku}-{colorValue.Value.ToUpper()}-{sizeValue.Value}",
                    OverrideCost = v == 0 ? null : new MoneyDto { Amount = product.Cost!.Amount + 10000, Currency = "VND" },
                    OverridePrice = v == 0 ? null : new MoneyDto { Amount = product.Price!.Amount + 15000, Currency = "VND" },
                    Dimensions = null,
                    MainImage = $"https://picsum.photos/seed/variant{i}-{v}/400/300",
                    Images = 
                    [
                        $"https://picsum.photos/seed/variant{i}-{v}-1/800/600",
                        $"https://picsum.photos/seed/variant{i}-{v}-2/800/600"
                    ],
                    AttributeValues = 
                    [
                        new VariantAttributeValueDto 
                        { 
                            ProductAttributeId = colorAttributeId,
                            AttributeName = "Color", 
                            ValueId = colorValue.Id,
                            ValueName = colorValue.Value,
                            ColorCode = colorValue.ColorCode
                        },
                        new VariantAttributeValueDto 
                        { 
                            ProductAttributeId = sizeAttributeId,
                            AttributeName = "Size", 
                            ValueId = sizeValue.Id,
                            ValueName = sizeValue.Value
                        }
                    ]
                };
                
                product.Variants.Add(variant);
            }
            
            _products.Add(product);
        }
    }
    
    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        await Task.Delay(300); // Simulate network delay
        return _products.FirstOrDefault(p => p.Id == id);
    }
    
    public async Task<ProductDto?> GetProductBySlugAsync(string slug)
    {
        await Task.Delay(300); // Simulate network delay
        return _products.FirstOrDefault(p => p.Slug == slug);
    }

    public async Task<PaginatedResult<ProductSearchDto>> SearchProductsAsync(
        string? keyword = null, 
        string? categoryId = null, 
        string? brandId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 20)
    {
        await Task.Delay(300); // Simulate network delay
        
        // Exclude drafts from search - use GetDraftsAsync for drafts
        var query = _products.Where(p => p.Status != "Draft").AsQueryable();
        
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId))
            query = query.Where(p => p.Category.Id == catId);
        
        if (!string.IsNullOrEmpty(brandId) && Guid.TryParse(brandId, out var brId))
            query = query.Where(p => p.Brand.Id == brId);
            
        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        
        var total = query.Count();
        var items = query
            .OrderBy(p => p.DisplayPriority)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductSearchDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Slug = p.Slug!,
                Sku = p.Sku,
                Price = p.Price,
                CategoryName = p.Category.Name,
                BrandName = p.Brand.Name,
                Thumbnail = p.Thumbnail,
                Status = p.Status,
                VariantCount = p.Variants.Count,
                CreatedAt = p.CreatedAt.DateTime
            })
            .ToList();
        
        return new PaginatedResult<ProductSearchDto>(page, pageSize, items, total);
    }
    
    public async Task<Guid> CreateDraftAsync(CreateProductDraftRequest request)
    {
        await Task.Delay(300);
        
        var category = _categories.FirstOrDefault(c => c.Id == request.CategoryId);
        var brand = _brands.FirstOrDefault(b => b.Id == request.BrandId);
        
        var product = new ProductDto
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Slug = request.Name.ToLower().Replace(" ", "-"),
            Sku = request.Sku,
            Cost = request.Cost,
            Price = request.Price,
            Dimensions = request.Dimensions,
            HasStockQuantity = request.HasStockQuantity,
            Thumbnail = request.Thumbnail,
            Images = request.Images,
            Category = category ?? new CategoryDto(),
            Brand = brand ?? new BrandDto(),
            DisplayPriority = request.DisplayPriority,
            Status = "Draft",
            Attributes = [],
            Variants = [],
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = null
        };
        
        _products.Add(product);
        return product.Id;
    }
    
    public async Task PublishProductAsync(Guid productId)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            product.Status = "Published";
            product.UpdatedAt = DateTimeOffset.Now;
        }
    }
    
    public async Task RepublishProductAsync(Guid productId)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product != null && product.Status == "Discontinued")
        {
            product.Status = "Published";
            product.UpdatedAt = DateTimeOffset.Now;
        }
    }
    
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        await Task.Delay(100);
        return _categories.ToList();
    }
    
    public async Task<List<BrandDto>> GetBrandsAsync()
    {
        await Task.Delay(100);
        return _brands.ToList();
    }
    
    public async Task<List<AttributeDto>> GetAttributesAsync()
    {
        await Task.Delay(100);
        return _attributes.ToList();
    }
    
    // === Attributes Management ===
    
    public async Task AddProductAttributeAsync(string slug, Guid attributeId, int displayOrder, bool hasVariant)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Slug == slug);
        if (product == null) return;
        
        var attribute = _attributes.FirstOrDefault(a => a.Id == attributeId);
        if (attribute == null) return;
        
        // Check if already exists
        if (product.Attributes.Any(a => a.AttributeId == attributeId)) return;
        
        product.Attributes.Add(new ProductAttributeDto
        {
            AttributeId = attributeId,
            AttributeName = attribute.Name,
            DisplayOrder = displayOrder,
            HasVariant = hasVariant
        });
        product.UpdatedAt = DateTimeOffset.Now;
    }
    
    public async Task RemoveProductAttributeAsync(string slug, Guid attributeId)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Slug == slug);
        if (product == null) return;
        
        var attr = product.Attributes.FirstOrDefault(a => a.AttributeId == attributeId);
        if (attr != null)
        {
            product.Attributes.Remove(attr);
            product.UpdatedAt = DateTimeOffset.Now;
        }
    }
    
    // === Variants Management ===
    
    public async Task<Guid> AddVariantAsync(string slug, AddVariantRequest request)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Slug == slug);
        if (product == null) return Guid.Empty;
        
        var variantId = Guid.NewGuid();
        var variant = new VariantDto
        {
            Id = variantId,
            Name = request.Name,
            Sku = request.Sku,
            OverrideCost = request.OverrideCostAmount.HasValue 
                ? new MoneyDto { Amount = request.OverrideCostAmount.Value, Currency = "VND" } 
                : null,
            OverridePrice = request.OverridePriceAmount.HasValue 
                ? new MoneyDto { Amount = request.OverridePriceAmount.Value, Currency = "VND" } 
                : null,
            MainImage = request.MainImage,
            Images = request.Images,
            AttributeValues = request.AttributeValues.Select(kv =>
            {
                var attr = _attributes.FirstOrDefault(a => a.Id == kv.Key);
                var value = attr?.Values.FirstOrDefault(v => v.Id == kv.Value);
                return new VariantAttributeValueDto
                {
                    ProductAttributeId = kv.Key,
                    AttributeName = attr?.Name ?? "",
                    ValueId = kv.Value,
                    ValueName = value?.Value ?? "",
                    ColorCode = value?.ColorCode
                };
            }).ToList()
        };
        
        product.Variants.Add(variant);
        product.UpdatedAt = DateTimeOffset.Now;
        return variantId;
    }
    
    public async Task UpdateVariantAsync(string slug, Guid variantId, UpdateVariantRequest request)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Slug == slug);
        if (product == null) return;
        
        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null) return;
        
        variant.Name = request.Name;
        variant.Sku = request.Sku;
        variant.OverrideCost = request.OverrideCostAmount.HasValue 
            ? new MoneyDto { Amount = request.OverrideCostAmount.Value, Currency = "VND" } 
            : null;
        variant.OverridePrice = request.OverridePriceAmount.HasValue 
            ? new MoneyDto { Amount = request.OverridePriceAmount.Value, Currency = "VND" } 
            : null;
        variant.MainImage = request.MainImage;
        variant.Images = request.Images;
        variant.AttributeValues = request.AttributeValues.Select(kv =>
        {
            var attr = _attributes.FirstOrDefault(a => a.Id == kv.Key);
            var value = attr?.Values.FirstOrDefault(v => v.Id == kv.Value);
            return new VariantAttributeValueDto
            {
                ProductAttributeId = kv.Key,
                AttributeName = attr?.Name ?? "",
                ValueId = kv.Value,
                ValueName = value?.Value ?? "",
                ColorCode = value?.ColorCode
            };
        }).ToList();
        
        product.UpdatedAt = DateTimeOffset.Now;
    }
    
    public async Task RemoveVariantAsync(string slug, Guid variantId)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Slug == slug);
        if (product == null) return;
        
        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant != null)
        {
            product.Variants.Remove(variant);
            product.UpdatedAt = DateTimeOffset.Now;
        }
    }
    
    // === Images Management ===
    
    public async Task UpdateThumbnailAsync(string slug, string thumbnailUrl)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Slug == slug);
        if (product == null) return;
        
        product.Thumbnail = thumbnailUrl;
        product.UpdatedAt = DateTimeOffset.Now;
    }
    
    public async Task AddImageAsync(string slug, string imageUrl)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Slug == slug);
        if (product == null) return;
        
        if (!product.Images.Contains(imageUrl))
        {
            product.Images.Add(imageUrl);
            product.UpdatedAt = DateTimeOffset.Now;
        }
    }
    
    public async Task RemoveImageAsync(string slug, string imageUrl)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Slug == slug);
        if (product == null) return;
        
        if (product.Images.Remove(imageUrl))
        {
            product.UpdatedAt = DateTimeOffset.Now;
        }
    }
    
    // === Draft Management ===
    
    public async Task<List<ProductSearchDto>> GetDraftsAsync()
    {
        await Task.Delay(200);
        return _products
            .Where(p => p.Status == "Draft")
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductSearchDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug ?? "",
                Sku = p.Sku,
                Thumbnail = p.Thumbnail,
                CategoryName = p.Category.Name,
                BrandName = p.Brand.Name,
                Price = p.Price,
                Status = p.Status,
                VariantCount = p.Variants.Count,
                CreatedAt = p.CreatedAt.DateTime
            })
            .ToList();
    }
    
    public async Task<ProductDto> CreateNewDraftAsync()
    {
        await Task.Delay(300);
        
        var newId = Guid.CreateVersion7();
        var defaultCategory = _categories.FirstOrDefault() ?? new CategoryDto { Id = Guid.Empty, Name = "Uncategorized" };
        var defaultBrand = _brands.FirstOrDefault() ?? new BrandDto { Id = Guid.Empty, Name = "Unknown" };
        
        var draft = new ProductDto
        {
            Id = newId,
            Name = "",
            Description = null,
            Slug = $"draft-{newId.ToString()[..8]}",
            Sku = $"SKU-{newId.ToString()[..8].ToUpper()}",
            Cost = new MoneyDto { Amount = 0, Currency = "VND" },
            Price = new MoneyDto { Amount = 0, Currency = "VND" },
            Dimensions = new DimensionsDto { Width = 10, Height = 10, Depth = 10, Weight = 1 },
            HasStockQuantity = true,
            Thumbnail = null,
            Images = [],
            Category = defaultCategory,
            Brand = defaultBrand,
            DisplayPriority = 1,
            Status = "Draft",
            Attributes = [],
            Variants = [],
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = null
        };
        
        _products.Add(draft);
        return draft;
    }
    
    public async Task UpdateDraftAsync(Guid productId, UpdateProductDraftRequest request)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product == null || product.Status != "Draft") return;
        
        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.Cost = new MoneyDto { Amount = request.CostAmount, Currency = "VND" };
        product.Price = new MoneyDto { Amount = request.PriceAmount, Currency = "VND" };
        product.Dimensions = new DimensionsDto
        {
            Width = request.Width,
            Height = request.Height,
            Depth = request.Depth,
            Weight = request.Weight
        };
        
        // Update category
        var category = _categories.FirstOrDefault(c => c.Id == request.CategoryId);
        if (category != null) product.Category = category;
        
        // Update brand
        var brand = _brands.FirstOrDefault(b => b.Id == request.BrandId);
        if (brand != null) product.Brand = brand;
        
        product.Thumbnail = request.Thumbnail;
        product.Images = request.Images ?? [];
        
        // Update slug based on name
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            product.Slug = GenerateSlug(request.Name);
        }
        
        product.UpdatedAt = DateTimeOffset.Now;
    }
    
    public async Task DiscardDraftAsync(Guid productId)
    {
        await Task.Delay(300);
        var product = _products.FirstOrDefault(p => p.Id == productId && p.Status == "Draft");
        if (product != null)
        {
            _products.Remove(product);
        }
    }
    
    private string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("đ", "d")
            .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
            .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
            .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
            .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
            .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
            .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
            .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");
    }
}
