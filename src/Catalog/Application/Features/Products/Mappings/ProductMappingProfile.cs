using Catalog.Domain;

namespace Catalog.Application;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku.Value))
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
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.DefaultValueId))
            .ForMember(dest => dest.ValueName, opt => opt.MapFrom(src => src.DefaultValue!.Name))
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
}
