using Catalog.Domain;
using Catalog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class CreateSampleData
{
    public record Command(int ProductCount = 36) : IRequest;

    public class Handler(CatalogSeeder seeder, CatalogDbContext dbContext, ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        private static readonly Random _random = new(42); // Fixed seed for reproducibility

        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            // Check if products already exist (idempotent)
            if (await dbContext.Products.AnyAsync(cancellationToken))
                return;

            // 1. Seed master data via CatalogSeeder (attributes, brands, categories)
            await seeder.SeedAsync();

            // 2. Load master data from DB
            var attributes = await dbContext.Attributes
                .Include(a => a.Values)
                .ToListAsync(cancellationToken);
            var brands = await dbContext.Brands.ToListAsync(cancellationToken);
            var categories = await dbContext.Categories
                .Include(c => c.DefaultAttributes)
                .Where(c => c.ParentId != null) // Only leaf categories
                .ToListAsync(cancellationToken);

            var attrMap = attributes.ToDictionary(a => a.Name);
            var brandMap = brands.ToDictionary(b => b.Name);
            var catMap = categories.ToDictionary(c => c.Name);

            // 3. Create products per category
            await CreateFashionProducts(attrMap, brandMap, catMap, cancellationToken);
            await CreateDeviceProducts(attrMap, brandMap, catMap, cancellationToken);
            await CreateElectronicsProducts(attrMap, brandMap, catMap, cancellationToken);
        }

        #region Fashion Products

        private async Task CreateFashionProducts(
            Dictionary<string, Domain.Attribute> attrs,
            Dictionary<string, Brand> brands,
            Dictionary<string, Category> cats,
            CancellationToken ct)
        {
            var colorAttr = attrs["Color"];
            var sizeAttr = attrs["Size"];

            // --- Shirt Products ---
            var shirtTemplates = new[]
            {
                ("Essential Crew Neck Tee", "Uniqlo", 199_000, 499_000),
                ("Slim Fit Oxford Shirt", "Zara", 299_000, 699_000),
                ("Oversized Graphic Tee", "H&M", 149_000, 399_000),
                ("Classic Linen Shirt", "Uniqlo", 349_000, 799_000),
                ("Striped Polo Shirt", "Levi's", 249_000, 599_000),
                ("Premium Cotton Henley", "Zara", 279_000, 649_000),
            };

            foreach (var (name, brand, minPrice, maxPrice) in shirtTemplates)
            {
                await CreateFashionProduct(
                    name, $"Comfortable and stylish {name.ToLower()}. Made from premium materials for everyday wear.",
                    cats["Shirt"], brands[brand], colorAttr, sizeAttr,
                    minPrice, maxPrice,
                    new Dimensions(0.3m, 0.4m, 0.05m, 0.2m),
                    "shirt", 6, ct);
            }

            // --- Jacket Products ---
            var jacketTemplates = new[]
            {
                ("Lightweight Windbreaker", "Nike", 599_000, 1_299_000),
                ("Puffer Down Jacket", "Adidas", 899_000, 1_999_000),
                ("Classic Denim Jacket", "Puma", 699_000, 1_499_000),
                ("Tech Fleece Hoodie", "Nike", 799_000, 1_699_000),
            };

            foreach (var (name, brand, minPrice, maxPrice) in jacketTemplates)
            {
                await CreateFashionProduct(
                    name, $"Stay warm and stylish with this {name.ToLower()}. Perfect for layering in any season.",
                    cats["Jacket"], brands[brand], colorAttr, sizeAttr,
                    minPrice, maxPrice,
                    new Dimensions(0.5m, 0.6m, 0.1m, 0.5m),
                    "jacket", 5, ct);
            }

            // --- Shoe Products ---
            var shoeTemplates = new[]
            {
                ("Air Max Runner", "Nike", 1_499_000, 3_499_000),
                ("Ultra Boost 23", "Adidas", 1_799_000, 3_999_000),
                ("Classic Leather Sneaker", "Puma", 999_000, 2_499_000),
                ("Retro Court Shoe", "Nike", 1_299_000, 2_999_000),
            };

            foreach (var (name, brand, minPrice, maxPrice) in shoeTemplates)
            {
                await CreateFashionProduct(
                    name, $"Engineered for comfort and performance. The {name.ToLower()} combines style with functionality.",
                    cats["Shoe"], brands[brand], colorAttr, sizeAttr,
                    minPrice, maxPrice,
                    new Dimensions(0.3m, 0.12m, 0.1m, 0.4m),
                    "shoe", 5, ct);
            }

            // --- Pants Products ---
            var pantsTemplates = new[]
            {
                ("Stretch Slim Chinos", "Uniqlo", 399_000, 799_000),
                ("Relaxed Fit Jeans", "Levi's", 499_000, 999_000),
                ("Cargo Utility Pants", "Zara", 449_000, 899_000),
                ("Tapered Jogger Pants", "Uniqlo", 349_000, 749_000),
            };

            foreach (var (name, brand, minPrice, maxPrice) in pantsTemplates)
            {
                await CreateFashionProduct(
                    name, $"Versatile {name.ToLower()} designed for comfort and modern style. Perfect for both casual and smart-casual occasions.",
                    cats["Pants"], brands[brand], colorAttr, sizeAttr,
                    minPrice, maxPrice,
                    new Dimensions(0.4m, 0.5m, 0.05m, 0.35m),
                    "pants", 5, ct);
            }
        }

        private async Task CreateFashionProduct(
            string name, string description,
            Category category, Brand brand,
            Domain.Attribute colorAttr, Domain.Attribute sizeAttr,
            int minPrice, int maxPrice,
            Dimensions dimensions, string imageCategory,
            int variantCount, CancellationToken ct)
        {
            var price = new Money(_random.Next(minPrice, maxPrice));
            var cost = new Money((int)(price.Amount * 0.6m));
            var sku = GenerateSku(category.Name, name);

            var product = Product.CreateDraft(
                Guid.NewGuid(), name, description, sku,
                cost, price, dimensions, true, category, brand.Id);

            // Thumbnail & images
            var thumbnail = $"https://picsum.photos/seed/{imageCategory}-{sku}/400/400.jpg";
            product.UpdateThumbnail(new ImageUrl(thumbnail), false);
            for (int i = 1; i <= 3; i++)
            {
                try { product.AddImage(new ImageUrl($"https://picsum.photos/seed/{imageCategory}-{sku}-{i}/400/400.jpg")); } catch { }
            }

            // Add attributes
            var colorValues = colorAttr.Values.ToList();
            var sizeValues = sizeAttr.Values.ToList();
            product.AddAttribute(colorAttr.Id, colorValues[0].Id, 1, hasVariant: true, raiseEvent: false);
            product.AddAttribute(sizeAttr.Id, sizeValues[0].Id, 2, hasVariant: true, raiseEvent: false);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(ct);

            // Load product attributes after save
            var productAttributes = await dbContext.ProductAttributes
                .Where(pa => pa.ProductId == product.Id)
                .ToListAsync(ct);

            var colorProdAttr = productAttributes.First(pa => pa.AttributeId == colorAttr.Id);
            var sizeProdAttr = productAttributes.First(pa => pa.AttributeId == sizeAttr.Id);

            // Create diverse Color × Size variants
            var selectedColors = colorValues.OrderBy(_ => _random.Next()).Take(variantCount > 4 ? 3 : 2).ToList();
            var selectedSizes = sizeValues.OrderBy(_ => _random.Next()).Take(variantCount > 4 ? 3 : 2).ToList();

            int variantsCreated = 0;
            foreach (var color in selectedColors)
            {
                foreach (var size in selectedSizes)
                {
                    if (variantsCreated >= variantCount) break;

                    Money? overridePrice = null;
                    Money? overrideCost = null;
                    // Larger sizes cost more
                    var sizeIdx = sizeValues.IndexOf(size);
                    if (sizeIdx >= 4) // XL+
                    {
                        overridePrice = new Money((int)(price.Amount * (1 + sizeIdx * 0.05m)));
                        overrideCost = new Money((int)(cost.Amount * (1 + sizeIdx * 0.05m)));
                    }

                    var variant = new Variant(
                        $"{color.Name} - {size.Name}",
                        new Sku($"{sku}-{color.Name[..1]}{size.Name}"),
                        overrideCost, overridePrice,
                        new ImageUrl($"https://picsum.photos/seed/{imageCategory}-{sku}-{color.Name}/400/400.jpg"),
                        null,
                        [(colorProdAttr, color.Id), (sizeProdAttr, size.Id)]
                    );

                    product.AddVariant(variant, false);
                    variantsCreated++;
                }
                if (variantsCreated >= variantCount) break;
            }

            // Featured chance
            if (_random.Next(100) < 20)
                product.UpdateDisplayPriority(0);

            // Publish → events → ES sync
            product.Publish();
            await uow.CommitAsync(ct);
        }

        #endregion

        #region Device Products

        private async Task CreateDeviceProducts(
            Dictionary<string, Domain.Attribute> attrs,
            Dictionary<string, Brand> brands,
            Dictionary<string, Category> cats,
            CancellationToken ct)
        {
            var colorAttr = attrs["Color"];
            var ramAttr = attrs["RAM"];
            var storageAttr = attrs["Storage"];
            var cpuAttr = attrs["CPU"];

            // --- Laptop Products ---
            var laptopTemplates = new[]
            {
                ("XPS 15 Pro", "Dell", 22_000_000, 45_000_000, "Intel Core i7"),
                ("MacBook Air M3", "Apple", 25_000_000, 42_000_000, "Apple M3"),
                ("ThinkPad X1 Carbon", "Lenovo", 20_000_000, 38_000_000, "Intel Core i7"),
                ("ROG Zephyrus G14", "ASUS", 28_000_000, 50_000_000, "AMD Ryzen 9"),
                ("Spectre x360", "HP", 24_000_000, 40_000_000, "Intel Core i5"),
            };

            foreach (var (name, brand, minPrice, maxPrice, defaultCpu) in laptopTemplates)
            {
                await CreateLaptopProduct(name, brand, minPrice, maxPrice, defaultCpu,
                    cats["Laptop"], brands[brand], ramAttr, storageAttr, cpuAttr, ct);
            }

            // --- SmartPhone Products ---
            var phoneTemplates = new[]
            {
                ("iPhone 16 Pro", "Apple", 25_000_000, 40_000_000),
                ("Galaxy S25 Ultra", "Samsung", 22_000_000, 35_000_000),
                ("Xperia 1 VI", "Sony", 18_000_000, 30_000_000),
                ("iPhone 16", "Apple", 20_000_000, 32_000_000),
                ("Galaxy Z Fold 6", "Samsung", 35_000_000, 50_000_000),
            };

            foreach (var (name, brand, minPrice, maxPrice) in phoneTemplates)
            {
                await CreatePhoneProduct(name,
                    cats["SmartPhone"], brands[brand], colorAttr, storageAttr,
                    minPrice, maxPrice, ct);
            }

            // --- Tablet Products ---
            var tabletTemplates = new[]
            {
                ("iPad Pro M4", "Apple", 22_000_000, 38_000_000),
                ("Galaxy Tab S10", "Samsung", 15_000_000, 28_000_000),
                ("iPad Air", "Apple", 16_000_000, 25_000_000),
            };

            foreach (var (name, brand, minPrice, maxPrice) in tabletTemplates)
            {
                await CreatePhoneProduct(name,
                    cats["Tablet"], brands[brand], colorAttr, storageAttr,
                    minPrice, maxPrice, ct);
            }
        }

        private async Task CreateLaptopProduct(
            string name, string brandName,
            int minPrice, int maxPrice, string defaultCpu,
            Category category, Brand brand,
            Domain.Attribute ramAttr, Domain.Attribute storageAttr, Domain.Attribute cpuAttr,
            CancellationToken ct)
        {
            var price = new Money(_random.Next(minPrice, maxPrice));
            var cost = new Money((int)(price.Amount * 0.7m));
            var sku = GenerateSku(category.Name, name);

            var product = Product.CreateDraft(
                Guid.NewGuid(), name,
                $"High performance {name} laptop. Powered by {defaultCpu} for seamless productivity and creative work.",
                sku, cost, price,
                new Dimensions(35m, 24m, 2m, 1.8m),
                true, category, brand.Id);

            product.UpdateThumbnail(new ImageUrl($"https://picsum.photos/seed/laptop-{sku}/400/400.jpg"), false);
            for (int i = 1; i <= 3; i++)
            {
                try { product.AddImage(new ImageUrl($"https://picsum.photos/seed/laptop-{sku}-{i}/400/400.jpg")); } catch { }
            }

            var ramValues = ramAttr.Values.ToList();
            var storageValues = storageAttr.Values.ToList();

            product.AddAttribute(ramAttr.Id, ramValues[0].Id, 1, hasVariant: true, raiseEvent: false);
            product.AddAttribute(storageAttr.Id, storageValues[0].Id, 2, hasVariant: true, raiseEvent: false);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(ct);

            var prodAttrs = await dbContext.ProductAttributes
                .Where(pa => pa.ProductId == product.Id)
                .ToListAsync(ct);

            var ramProdAttr = prodAttrs.First(pa => pa.AttributeId == ramAttr.Id);
            var storageProdAttr = prodAttrs.First(pa => pa.AttributeId == storageAttr.Id);

            // RAM × Storage variants (3-4)
            var ramOptions = ramValues.Where(v => v.Name is "8GB" or "16GB" or "32GB").ToList();
            var storageOptions = storageValues.Where(v => v.Name is "256GB" or "512GB" or "1TB").ToList();

            int created = 0;
            foreach (var ram in ramOptions)
            {
                foreach (var storage in storageOptions)
                {
                    if (created >= 4) break;

                    var multiplier = 1m + (ramOptions.IndexOf(ram) * 0.15m) + (storageOptions.IndexOf(storage) * 0.1m);

                    var variant = new Variant(
                        $"{ram.Name} - {storage.Name}",
                        new Sku($"{sku}-{ram.Name.Replace("GB", "")}-{storage.Name}"),
                        created == 0 ? null : new Money((int)(cost.Amount * multiplier)),
                        created == 0 ? null : new Money((int)(price.Amount * multiplier)),
                        new ImageUrl($"https://picsum.photos/seed/laptop-{sku}-v{created}/400/400.jpg"),
                        null,
                        [(ramProdAttr, ram.Id), (storageProdAttr, storage.Id)]
                    );

                    product.AddVariant(variant, false);
                    created++;
                }
                if (created >= 4) break;
            }

            if (_random.Next(100) < 20)
                product.UpdateDisplayPriority(0);

            product.Publish();
            await uow.CommitAsync(ct);
        }

        private async Task CreatePhoneProduct(
            string name,
            Category category, Brand brand,
            Domain.Attribute colorAttr, Domain.Attribute storageAttr,
            int minPrice, int maxPrice,
            CancellationToken ct)
        {
            var price = new Money(_random.Next(minPrice, maxPrice));
            var cost = new Money((int)(price.Amount * 0.65m));
            var sku = GenerateSku(category.Name, name);
            var catLabel = category.Name == "SmartPhone" ? "smartphone" : "tablet";

            var product = Product.CreateDraft(
                Guid.NewGuid(), name,
                $"Experience the next generation {name}. Stunning display, powerful performance, and all-day battery life.",
                sku, cost, price,
                category.Name == "SmartPhone"
                    ? new Dimensions(0.16m, 0.08m, 0.008m, 0.2m)
                    : new Dimensions(0.25m, 0.17m, 0.006m, 0.45m),
                true, category, brand.Id);

            product.UpdateThumbnail(new ImageUrl($"https://picsum.photos/seed/{catLabel}-{sku}/400/400.jpg"), false);
            for (int i = 1; i <= 3; i++)
            {
                try { product.AddImage(new ImageUrl($"https://picsum.photos/seed/{catLabel}-{sku}-{i}/400/400.jpg")); } catch { }
            }

            var colorValues = colorAttr.Values.ToList();
            var storageValues = storageAttr.Values.ToList();

            product.AddAttribute(colorAttr.Id, colorValues[0].Id, 1, hasVariant: true, raiseEvent: false);
            product.AddAttribute(storageAttr.Id, storageValues[0].Id, 2, hasVariant: true, raiseEvent: false);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(ct);

            var prodAttrs = await dbContext.ProductAttributes
                .Where(pa => pa.ProductId == product.Id)
                .ToListAsync(ct);

            var colorProdAttr = prodAttrs.First(pa => pa.AttributeId == colorAttr.Id);
            var storageProdAttr = prodAttrs.First(pa => pa.AttributeId == storageAttr.Id);

            // Color × Storage variants (3-4)
            var selectedColors = colorValues
                .Where(v => v.Name is "Black" or "White" or "Blue" or "Purple")
                .OrderBy(_ => _random.Next()).Take(2).ToList();
            var selectedStorages = storageValues
                .Where(v => v.Name is "128GB" or "256GB" or "512GB")
                .ToList();

            int created = 0;
            foreach (var color in selectedColors)
            {
                foreach (var storage in selectedStorages)
                {
                    if (created >= 4) break;

                    var storageMultiplier = 1m + (selectedStorages.IndexOf(storage) * 0.15m);

                    var variant = new Variant(
                        $"{color.Name} - {storage.Name}",
                        new Sku($"{sku}-{color.Name[..1]}{storage.Name}"),
                        created == 0 ? null : new Money((int)(cost.Amount * storageMultiplier)),
                        created == 0 ? null : new Money((int)(price.Amount * storageMultiplier)),
                        new ImageUrl($"https://picsum.photos/seed/{catLabel}-{sku}-{color.Name}/400/400.jpg"),
                        null,
                        [(colorProdAttr, color.Id), (storageProdAttr, storage.Id)]
                    );

                    product.AddVariant(variant, false);
                    created++;
                }
                if (created >= 4) break;
            }

            if (_random.Next(100) < 20)
                product.UpdateDisplayPriority(0);

            product.Publish();
            await uow.CommitAsync(ct);
        }

        #endregion

        #region Electronics Products

        private async Task CreateElectronicsProducts(
            Dictionary<string, Domain.Attribute> attrs,
            Dictionary<string, Brand> brands,
            Dictionary<string, Category> cats,
            CancellationToken ct)
        {
            var colorAttr = attrs["Color"];

            // --- Headphones ---
            var headphoneTemplates = new[]
            {
                ("WH-1000XM5", "Sony", 5_500_000, 8_500_000),
                ("Galaxy Buds3 Pro", "Samsung", 3_500_000, 5_000_000),
                ("AirPods Pro 3", "Apple", 5_000_000, 7_000_000),
            };

            foreach (var (name, brand, minPrice, maxPrice) in headphoneTemplates)
            {
                await CreateColorOnlyProduct(name,
                    $"Immersive sound experience with industry-leading noise cancellation. The {name} redefines audio quality.",
                    cats["Headphones"], brands[brand], colorAttr,
                    minPrice, maxPrice,
                    new Dimensions(0.2m, 0.18m, 0.08m, 0.25m),
                    "headphones", 3, ct);
            }

            // --- Smart Watch ---
            var watchTemplates = new[]
            {
                ("Apple Watch Ultra 3", "Apple", 18_000_000, 25_000_000),
                ("Galaxy Watch 7", "Samsung", 6_000_000, 12_000_000),
            };

            foreach (var (name, brand, minPrice, maxPrice) in watchTemplates)
            {
                await CreateColorOnlyProduct(name,
                    $"Your ultimate health and fitness companion. The {name} features advanced sensors and all-day battery.",
                    cats["Smart Watch"], brands[brand], colorAttr,
                    minPrice, maxPrice,
                    new Dimensions(0.05m, 0.05m, 0.01m, 0.05m),
                    "smartwatch", 3, ct);
            }
        }

        private async Task CreateColorOnlyProduct(
            string name, string description,
            Category category, Brand brand,
            Domain.Attribute colorAttr,
            int minPrice, int maxPrice,
            Dimensions dimensions, string imageCategory,
            int variantCount, CancellationToken ct)
        {
            var price = new Money(_random.Next(minPrice, maxPrice));
            var cost = new Money((int)(price.Amount * 0.65m));
            var sku = GenerateSku(category.Name, name);

            var product = Product.CreateDraft(
                Guid.NewGuid(), name, description, sku,
                cost, price, dimensions, true, category, brand.Id);

            product.UpdateThumbnail(new ImageUrl($"https://picsum.photos/seed/{imageCategory}-{sku}/400/400.jpg"), false);
            for (int i = 1; i <= 3; i++)
            {
                try { product.AddImage(new ImageUrl($"https://picsum.photos/seed/{imageCategory}-{sku}-{i}/400/400.jpg")); } catch { }
            }

            var colorValues = colorAttr.Values.ToList();
            product.AddAttribute(colorAttr.Id, colorValues[0].Id, 1, hasVariant: true, raiseEvent: false);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(ct);

            var prodAttrs = await dbContext.ProductAttributes
                .Where(pa => pa.ProductId == product.Id)
                .ToListAsync(ct);

            var colorProdAttr = prodAttrs.First(pa => pa.AttributeId == colorAttr.Id);

            var selectedColors = colorValues
                .Where(v => v.Name is "Black" or "White" or "Gray" or "Blue")
                .OrderBy(_ => _random.Next())
                .Take(variantCount)
                .ToList();

            foreach (var color in selectedColors)
            {
                var variant = new Variant(
                    color.Name,
                    new Sku($"{sku}-{color.Name[..2]}"),
                    null, null,
                    new ImageUrl($"https://picsum.photos/seed/{imageCategory}-{sku}-{color.Name}/400/400.jpg"),
                    null,
                    [(colorProdAttr, color.Id)]
                );

                product.AddVariant(variant, false);
            }

            if (_random.Next(100) < 20)
                product.UpdateDisplayPriority(0);

            product.Publish();
            await uow.CommitAsync(ct);
        }

        #endregion

        #region Helpers

        private static string GenerateSku(string category, string name)
        {
            var catPrefix = category.Length >= 3 ? category[..3].ToUpper() : category.ToUpper();
            var nameClean = string.Concat(name.Split(' ').Select(w => w.Length > 0 ? w[..1].ToString().ToUpper() : ""));
            return $"{catPrefix}-{nameClean}";
        }

        #endregion
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products/create-sample-data", async (ISender sender) =>
            {
                await sender.Send(new Command());
                return Results.Ok(new { Message = "Sample data creation initiated." });
            })
            .WithTags("Products")
            .WithName("CreateSampleData")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
