using Catalog.Domain;
using Catalog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class CreateSampleData
{
    public record Command(int ProductCount = 36) : IRequest;

    public class Handler(CatalogDbContext dbContext) : IRequestHandler<Command>
    {
        // Shirt product images
        private static readonly string[] ShirtImages =
        [
            "https://picsum.photos/seed/shirt1/400/400.jpg",
            "https://picsum.photos/seed/shirt2/400/400.jpg",
            "https://picsum.photos/seed/shirt3/400/400.jpg",
            "https://picsum.photos/seed/shirt4/400/400.jpg",
            "https://picsum.photos/seed/shirt5/400/400.jpg",
            "https://picsum.photos/seed/shirt6/400/400.jpg",
        ];

        // Laptop product images
        private static readonly string[] LaptopImages =
        [
            "https://picsum.photos/seed/laptop1/400/400.jpg",
            "https://picsum.photos/seed/laptop2/400/400.jpg",
            "https://picsum.photos/seed/laptop3/400/400.jpg",
            "https://picsum.photos/seed/laptop4/400/400.jpg",
            "https://picsum.photos/seed/laptop5/400/400.jpg",
            "https://picsum.photos/seed/laptop6/400/400.jpg",
        ];

        private static readonly Random _random = new();

        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            // Check if sample data already exists
            if (await dbContext.Brands.AnyAsync(cancellationToken))
            {
                return; // Sample data already created
            }

            // 1. Create Attributes with Values
            var colorAttr = new Domain.Attribute
            {
                Name = "Color",
                Icon = "palette",
                DisplayText = false,
                ValueStyleCss = "width: 24px; height: 24px; border-radius: 50%;"
            };
            colorAttr.AddValue("White", "#FFFFFF");
            colorAttr.AddValue("Black", "#000000");

            var sizeAttr = new Domain.Attribute
            {
                Name = "Size",
                Icon = "straighten",
                DisplayText = true,
                ValueStyleCss = null
            };
            sizeAttr.AddValue("M", null);
            sizeAttr.AddValue("L", null);

            var ramAttr = new Domain.Attribute
            {
                Name = "RAM",
                Icon = "memory",
                DisplayText = true,
                ValueStyleCss = null
            };
            ramAttr.AddValue("4GB", null);
            ramAttr.AddValue("8GB", null);

            var storageAttr = new Domain.Attribute
            {
                Name = "Storage",
                Icon = "storage",
                DisplayText = true,
                ValueStyleCss = null
            };
            storageAttr.AddValue("128GB", null);
            storageAttr.AddValue("256GB", null);

            dbContext.Attributes.AddRange(colorAttr, sizeAttr, ramAttr, storageAttr);

            // 2. Create Brands
            var uniqlo = new Brand { Name = "Uniqlo", Logo = "https://picsum.photos/seed/uniqlo/100/100" };
            var dell = new Brand { Name = "DELL", Logo = "https://picsum.photos/seed/dell/100/100" };

            dbContext.Brands.AddRange(uniqlo, dell);

            // 3. Create Categories with Default Attributes
            var shirtCategory = new Category { Name = "Shirt", Image = "https://picsum.photos/seed/cat-shirt/200/200" };
            var laptopCategory = new Category { Name = "Laptop", Image = "https://picsum.photos/seed/cat-laptop/200/200" };

            dbContext.Categories.AddRange(shirtCategory, laptopCategory);

            // Save to get IDs
            await dbContext.SaveChangesAsync(cancellationToken);

            // Set default attributes for categories
            shirtCategory.SetDefaultAttributes(
            [
                new CategoryDefaultAttribute { AttributeId = colorAttr.Id, DisplayOrder = 1 },
                new CategoryDefaultAttribute { AttributeId = sizeAttr.Id, DisplayOrder = 2 }
            ]);

            laptopCategory.SetDefaultAttributes(
            [
                new CategoryDefaultAttribute { AttributeId = ramAttr.Id, DisplayOrder = 1 },
                new CategoryDefaultAttribute { AttributeId = storageAttr.Id, DisplayOrder = 2 }
            ]);

            await dbContext.SaveChangesAsync(cancellationToken);

            // 4. Create Products
            var productCount = command.ProductCount;
            var shirtProductCount = productCount / 2;
            var laptopProductCount = productCount - shirtProductCount;

            // Create Shirt products
            for (int i = 0; i < shirtProductCount; i++)
            {
                await CreateShirtProduct(i, shirtCategory, uniqlo, colorAttr, sizeAttr, cancellationToken);
            }

            // Create Laptop products
            for (int i = 0; i < laptopProductCount; i++)
            {
                await CreateLaptopProduct(i, laptopCategory, dell, ramAttr, storageAttr, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task CreateShirtProduct(
            int index,
            Category category,
            Brand brand,
            Domain.Attribute colorAttr,
            Domain.Attribute sizeAttr,
            CancellationToken cancellationToken)
        {
            var productId = Guid.NewGuid();
            var price = new Money(_random.Next(199000, 799000));
            var cost = new Money((int)(price.Amount * 0.6m));

            var product = Product.CreateDraft(
                productId,
                $"Premium T-Shirt {index + 1}",
                $"High quality cotton t-shirt with modern design. Perfect for casual wear. Model {index + 1}.",
                $"SHIRT-{index + 1:D4}",
                cost,
                price,
                new Dimensions(0.3m, 0.4m, 0.05m, 0.2m),
                true,
                category,
                brand.Id
            );

            // Set thumbnail and images
            var thumbnailUrl = ShirtImages[_random.Next(ShirtImages.Length)];
            product.UpdateThumbnail(new ImageUrl(thumbnailUrl), false);

            foreach (var img in ShirtImages.OrderBy(_ => _random.Next()).Take(3))
            {
                try { product.AddImage(new ImageUrl(img)); } catch { }
            }

            // Add product attributes with HasVariant = true
            var colorValues = colorAttr.Values.ToList();
            var sizeValues = sizeAttr.Values.ToList();

            product.AddAttribute(colorAttr.Id, colorValues[0].Id, 1, hasVariant: true, raiseEvent: false);
            product.AddAttribute(sizeAttr.Id, sizeValues[0].Id, 2, hasVariant: true, raiseEvent: false);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Get ProductAttributes after save
            var productAttributes = await dbContext.ProductAttributes
                .Where(pa => pa.ProductId == product.Id)
                .ToListAsync(cancellationToken);

            var colorProdAttr = productAttributes.First(pa => pa.AttributeId == colorAttr.Id);
            var sizeProdAttr = productAttributes.First(pa => pa.AttributeId == sizeAttr.Id);

            // Create 2 variants (different color/size combinations)
            var variant1 = new Variant(
                $"White - M",
                new Sku($"SHIRT-{index + 1:D4}-WM"),
                null, null, new ImageUrl(ShirtImages[_random.Next(ShirtImages.Length)]), null,
                [(colorProdAttr, colorValues.First(v => v.Name == "White").Id),
                 (sizeProdAttr, sizeValues.First(v => v.Name == "M").Id)]
            );

            var variant2 = new Variant(
                $"Black - L",
                new Sku($"SHIRT-{index + 1:D4}-BL"),
                null, null, new ImageUrl(ShirtImages[_random.Next(ShirtImages.Length)]), null,
                [(colorProdAttr, colorValues.First(v => v.Name == "Black").Id),
                 (sizeProdAttr, sizeValues.First(v => v.Name == "L").Id)]
            );

            product.AddVariant(variant1, false);
            product.AddVariant(variant2, false);

            // Publish product
            product.Publish();
        }

        private async Task CreateLaptopProduct(
            int index,
            Category category,
            Brand brand,
            Domain.Attribute ramAttr,
            Domain.Attribute storageAttr,
            CancellationToken cancellationToken)
        {
            var productId = Guid.NewGuid();
            var price = new Money(_random.Next(15000000, 45000000));
            var cost = new Money((int)(price.Amount * 0.7m));

            var product = Product.CreateDraft(
                productId,
                $"DELL Laptop Pro {index + 1}",
                $"High performance laptop with powerful processor. Perfect for work and gaming. Model {index + 1}.",
                $"LAPTOP-{index + 1:D4}",
                cost,
                price,
                new Dimensions(35m, 24m, 2m, 1.8m),
                true,
                category,
                brand.Id
            );

            // Set thumbnail and images
            var thumbnailUrl = LaptopImages[_random.Next(LaptopImages.Length)];
            product.UpdateThumbnail(new ImageUrl(thumbnailUrl), false);

            foreach (var img in LaptopImages.OrderBy(_ => _random.Next()).Take(3))
            {
                try { product.AddImage(new ImageUrl(img)); } catch { }
            }

            // Add product attributes with HasVariant = true
            var ramValues = ramAttr.Values.ToList();
            var storageValues = storageAttr.Values.ToList();

            product.AddAttribute(ramAttr.Id, ramValues[0].Id, 1, hasVariant: true, raiseEvent: false);
            product.AddAttribute(storageAttr.Id, storageValues[0].Id, 2, hasVariant: true, raiseEvent: false);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Get ProductAttributes after save
            var productAttributes = await dbContext.ProductAttributes
                .Where(pa => pa.ProductId == product.Id)
                .ToListAsync(cancellationToken);

            var ramProdAttr = productAttributes.First(pa => pa.AttributeId == ramAttr.Id);
            var storageProdAttr = productAttributes.First(pa => pa.AttributeId == storageAttr.Id);

            // Create 2 variants (different RAM/Storage combinations)
            var variant1 = new Variant(
                $"4GB - 128GB",
                new Sku($"LAPTOP-{index + 1:D4}-4-128"),
                null, null, new ImageUrl(LaptopImages[_random.Next(LaptopImages.Length)]), null,
                [(ramProdAttr, ramValues.First(v => v.Name == "4GB").Id),
                 (storageProdAttr, storageValues.First(v => v.Name == "128GB").Id)]
            );

            var variant2 = new Variant(
                $"8GB - 256GB",
                new Sku($"LAPTOP-{index + 1:D4}-8-256"),
                new Money((int)(price.Amount * 1.2m)), // Override price for higher specs
                new Money((int)(cost.Amount * 1.2m)),
                new ImageUrl(LaptopImages[_random.Next(LaptopImages.Length)]), null,
                [(ramProdAttr, ramValues.First(v => v.Name == "8GB").Id),
                 (storageProdAttr, storageValues.First(v => v.Name == "256GB").Id)]
            );

            product.AddVariant(variant1, false);
            product.AddVariant(variant2, false);

            // Publish product
            product.Publish();
        }
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
