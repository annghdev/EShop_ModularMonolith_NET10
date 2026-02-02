using Catalog.Domain;
using Attribute = Catalog.Domain.Attribute;

namespace Catalog.Infrastructure;

public class CatalogSeeder
{
    private readonly CatalogDbContext _context;

    public CatalogSeeder(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedAttributesAsync();
        await SeedBrandsAsync();
        await _context.SaveChangesAsync();
        await SeedCategoriesAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedAttributesAsync()
    {
        if (_context.Attributes.Any()) return;

        // Color Attribute
        var colorAttribute = new Attribute
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Color",
            Icon = "🎨",
            DisplayText = true,
            Values = new List<AttributeValue>
            {
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), Name = "Red", ColorCode = "#FF0000" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111113"), Name = "Blue", ColorCode = "#0000FF" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111114"), Name = "Green", ColorCode = "#00FF00" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111115"), Name = "Black", ColorCode = "#000000" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111116"), Name = "White", ColorCode = "#FFFFFF" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111117"), Name = "Gray", ColorCode = "#808080" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111118"), Name = "Yellow", ColorCode = "#FFFF00" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111119"), Name = "Purple", ColorCode = "#800080" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111120"), Name = "Orange", ColorCode = "#FFA500" },
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111121"), Name = "Pink", ColorCode = "#FFC0CB" }
            }
        };

        // Size Attribute
        var sizeAttribute = new Attribute
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Size",
            Icon = "📏",
            DisplayText = true,
            Values = new List<AttributeValue>
            {
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222223"), Name = "XS" },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222224"), Name = "S" },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222225"), Name = "M" },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222226"), Name = "L" },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222227"), Name = "XL" },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222228"), Name = "XXL" },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222229"), Name = "XXXL" }
            }
        };

        // RAM Attribute
        var ramAttribute = new Attribute
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "RAM",
            Icon = "🧠",
            DisplayText = true,
            Values = new List<AttributeValue>
            {
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), Name = "4GB" },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333335"), Name = "8GB" },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333336"), Name = "16GB" },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333337"), Name = "32GB" },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333338"), Name = "64GB" },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333339"), Name = "128GB" }
            }
        };

        // CPU Attribute
        var cpuAttribute = new Attribute
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "CPU",
            Icon = "⚡",
            DisplayText = true,
            Values = new List<AttributeValue>
            {
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444445"), Name = "Intel Core i3" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444446"), Name = "Intel Core i5" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444447"), Name = "Intel Core i7" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444448"), Name = "Intel Core i9" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444449"), Name = "AMD Ryzen 3" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444450"), Name = "AMD Ryzen 5" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444451"), Name = "AMD Ryzen 7" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444452"), Name = "AMD Ryzen 9" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444453"), Name = "Apple M1" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444454"), Name = "Apple M2" },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444455"), Name = "Apple M3" }
            }
        };

        // Battery Attribute
        var batteryAttribute = new Attribute
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Name = "Battery",
            Icon = "🔋",
            DisplayText = true,
            Values = new List<AttributeValue>
            {
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555556"), Name = "2000mAh" },
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555557"), Name = "3000mAh" },
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555558"), Name = "4000mAh" },
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555559"), Name = "5000mAh" },
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555560"), Name = "6000mAh" },
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555561"), Name = "10000mAh" }
            }
        };

        // Storage Attribute
        var storageAttribute = new Attribute
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Name = "Storage",
            Icon = "💾",
            DisplayText = true,
            Values = new List<AttributeValue>
            {
                new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666667"), Name = "64GB" },
                new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666668"), Name = "128GB" },
                new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666669"), Name = "256GB" },
                new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666670"), Name = "512GB" },
                new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666671"), Name = "1TB" },
                new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666672"), Name = "2TB" }
            }
        };

        // Screen Size Attribute
        var screenSizeAttribute = new Attribute
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Name = "Screen Size",
            Icon = "📱",
            DisplayText = true,
            Values = new List<AttributeValue>
            {
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777778"), Name = "5.5 inch" },
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777779"), Name = "6.1 inch" },
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777780"), Name = "6.5 inch" },
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777781"), Name = "6.7 inch" },
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777782"), Name = "13 inch" },
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777783"), Name = "15.6 inch" },
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777784"), Name = "17.3 inch" }
            }
        };

        await _context.Attributes.AddRangeAsync(colorAttribute, sizeAttribute, ramAttribute, cpuAttribute, batteryAttribute, storageAttribute, screenSizeAttribute);
    }

    private async Task SeedBrandsAsync()
    {
        if (_context.Brands.Any()) return;

        var brands = new List<Brand>
        {
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Uniqlo" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac"), Name = "Apple" },
            new() { Id = Guid.NewGuid(), Name = "Nike" },
            new() { Id = Guid.NewGuid(), Name = "ASUS" },
            new() { Id = Guid.NewGuid(), Name = "Samsung" },
            new() { Id = Guid.NewGuid(), Name = "Sony" },
            new() { Id = Guid.NewGuid(), Name = "Microsoft" },
            new() { Id = Guid.NewGuid(), Name = "Dell" },
            new() { Id = Guid.NewGuid(), Name = "HP" },
            new() { Id = Guid.NewGuid(), Name = "Lenovo" },
            new() { Id = Guid.NewGuid(), Name = "Adidas" },
            new() { Id = Guid.NewGuid(), Name = "Puma" },
            new() { Id = Guid.NewGuid(), Name = "Zara" },
            new() { Id = Guid.NewGuid(), Name = "H&M" },
            new() { Id = Guid.NewGuid(), Name = "Levi's" }
        };

        await _context.Brands.AddRangeAsync(brands);
    }

    private async Task SeedCategoriesAsync()
    {
        if (_context.Categories.Any()) return;

        // Get attributes for category defaults
        var colorAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "Color")
            ?? throw new InvalidOperationException("Color attribute not found");
        var sizeAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "Size")
            ?? throw new InvalidOperationException("Size attribute not found");
        var ramAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "RAM")
            ?? throw new InvalidOperationException("RAM attribute not found");
        var cpuAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "CPU")
            ?? throw new InvalidOperationException("CPU attribute not found");
        var batteryAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "Battery")
            ?? throw new InvalidOperationException("Battery attribute not found");
        var storageAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "Storage")
            ?? throw new InvalidOperationException("Storage attribute not found");
        var screenSizeAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "Screen Size")
            ?? throw new InvalidOperationException("Screen Size attribute not found");

        // Fashion Category
        var fashionCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Fashion",
            Children = new List<Category>
            {
                new()
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc"),
                    Name = "Shirt",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = colorAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = sizeAttribute.Id, DisplayOrder = 2 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Jacket",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = colorAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = sizeAttribute.Id, DisplayOrder = 2 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Shoe",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = colorAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = sizeAttribute.Id, DisplayOrder = 2 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Pants",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = colorAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = sizeAttribute.Id, DisplayOrder = 2 }
                    }
                }
            }
        };

        // Device Category
        var deviceCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Device",
            Children = new List<Category>
            {
                new()
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccd"), // Used in sample requests
                    Name = "SmartPhone",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = colorAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = storageAttribute.Id, DisplayOrder = 2 },
                        new() { Id = Guid.NewGuid(), AttributeId = ramAttribute.Id, DisplayOrder = 3 },
                        new() { Id = Guid.NewGuid(), AttributeId = batteryAttribute.Id, DisplayOrder = 4 },
                        new() { Id = Guid.NewGuid(), AttributeId = screenSizeAttribute.Id, DisplayOrder = 5 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "PC",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = cpuAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = ramAttribute.Id, DisplayOrder = 2 },
                        new() { Id = Guid.NewGuid(), AttributeId = storageAttribute.Id, DisplayOrder = 3 },
                        new() { Id = Guid.NewGuid(), AttributeId = screenSizeAttribute.Id, DisplayOrder = 4 }
                    }
                },
                new()
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccb"), // Used in sample requests
                    Name = "Laptop",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = cpuAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = ramAttribute.Id, DisplayOrder = 2 },
                        new() { Id = Guid.NewGuid(), AttributeId = storageAttribute.Id, DisplayOrder = 3 },
                        new() { Id = Guid.NewGuid(), AttributeId = screenSizeAttribute.Id, DisplayOrder = 4 },
                        new() { Id = Guid.NewGuid(), AttributeId = batteryAttribute.Id, DisplayOrder = 5 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Tablet",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = colorAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = storageAttribute.Id, DisplayOrder = 2 },
                        new() { Id = Guid.NewGuid(), AttributeId = ramAttribute.Id, DisplayOrder = 3 },
                        new() { Id = Guid.NewGuid(), AttributeId = batteryAttribute.Id, DisplayOrder = 4 },
                        new() { Id = Guid.NewGuid(), AttributeId = screenSizeAttribute.Id, DisplayOrder = 5 }
                    }
                }
            }
        };

        // Electronics Category
        var electronicsCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Children = new List<Category>
            {
                new()
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddde"), // Used in sample requests
                    Name = "Headphones",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = colorAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = batteryAttribute.Id, DisplayOrder = 2 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Smart Watch",
                    DefaultAttributes = new List<CategoryDefaultAttribute>
                    {
                        new() { Id = Guid.NewGuid(), AttributeId = colorAttribute.Id, DisplayOrder = 1 },
                        new() { Id = Guid.NewGuid(), AttributeId = batteryAttribute.Id, DisplayOrder = 2 }
                    }
                }
            }
        };

        await _context.Categories.AddRangeAsync(fashionCategory, deviceCategory, electronicsCategory);
    }
}
