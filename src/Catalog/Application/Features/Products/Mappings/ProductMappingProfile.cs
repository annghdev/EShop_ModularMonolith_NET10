using Catalog.Domain;

namespace Catalog.Application;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug!.Value))
            .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost.ToMoneyDto()))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.ToMoneyDto()))
            .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => src.Dimensions.ToDimensionsDto()))
            .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail!.Path))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.Select(i => i.Path)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand));

        CreateMap<Category, CategoryDto>();
        CreateMap<Brand, BrandDto>();

        CreateMap<ProductAttribute, ProductAttributeDto>()
            .ForMember(dest => dest.AttributeId, opt => opt.MapFrom(src => src.AttributeId))
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.Attribute!.Name))
            .ForMember(dest => dest.HasVariant, opt => opt.MapFrom(src => src.HasVariant));

        CreateMap<Variant, VariantDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku.Value))
            .ForMember(dest => dest.OverrideCost, opt => opt.MapFrom(src => src.OverrideCost.ToMoneyDto()))
            .ForMember(dest => dest.OverridePrice, opt => opt.MapFrom(src => src.OverridePrice.ToMoneyDto()))
            .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => src.OverrideDimensions.ToDimensionsDto()));

        CreateMap<VariantAttributeValue, VariantAttributeValueDto>()
            .ForMember(dest => dest.ProductAttributeId, opt => opt.MapFrom(src => src.ProductAttributeId))
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.ProductAttribute!.Attribute!.Name))
            .ForMember(dest => dest.ValueId, opt => opt.MapFrom(src => src.ValueId))
            .ForMember(dest => dest.ValueName, opt => opt.MapFrom(src => src.Value!.Name));
    }

    public static ProductProjection MapToProjection(Product product)
    {
        return new ProductProjection
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Slug = product.Slug?.Value ?? string.Empty,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            CategoryId = product.CategoryId.ToString(),
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandId = product.BrandId.ToString(),
            BrandName = product.Brand?.Name ?? string.Empty,
            Status = product.Status.ToString(),
            Attributes = product.Attributes.Select(pa => new ProductAttributeProjection
            {
                AttributeId = pa.AttributeId.ToString(),
                AttributeName = pa.Attribute?.Name ?? string.Empty
            }).ToList(),
            Variants = product.Variants.Select(v => new ProductVariantProjection
            {
                Id = v.Id.ToString(),
                Name = v.Name,
                Sku = v.Sku.Value,
                Price = v.OverridePrice?.Amount ?? product.Price.Amount,
                Currency = v.OverridePrice?.Currency ?? product.Price.Currency,
                Attributes = v.AttributeValues.Select(av => new VariantAttributeProjection
                {
                    AttributeId = av.ProductAttributeId.ToString(),
                    AttributeName = av.ProductAttribute?.Attribute?.Name ?? string.Empty,
                    ValueId = av.ValueId.ToString(),
                    ValueName = av.Value?.Name ?? string.Empty
                }).ToList()
            }).ToList(),
            CreatedAt = product.CreatedAt.DateTime,
            UpdatedAt = product.UpdatedAt?.DateTime
        };
    }
}
