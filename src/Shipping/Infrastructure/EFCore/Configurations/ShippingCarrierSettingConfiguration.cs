using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Domain;

namespace Shipping.Infrastructure.EFCore.Configurations;

public class ShippingCarrierSettingConfiguration : IEntityTypeConfiguration<ShippingCarrierSetting>
{
    public void Configure(EntityTypeBuilder<ShippingCarrierSetting> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property<Guid>("ShippingCarrierId")
            .IsRequired();

        builder.Property(s => s.ApiToken)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.ShopId)
            .HasMaxLength(200);

        builder.Property(s => s.ApiUrl)
            .HasMaxLength(500);

        builder.Property(s => s.WebhookUrl)
            .HasMaxLength(500);

        builder.Property(s => s.AdditionalSettings)
            .HasColumnType("jsonb");

        builder.HasIndex("ShippingCarrierId")
            .IsUnique();
    }
}
