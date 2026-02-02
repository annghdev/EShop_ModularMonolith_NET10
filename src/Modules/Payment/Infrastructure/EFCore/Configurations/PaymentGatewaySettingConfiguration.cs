using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain;

namespace Payment.Infrastructure.EFCore.Configurations;

public class PaymentGatewaySettingConfiguration : IEntityTypeConfiguration<PaymentGatewaySetting>
{
    public void Configure(EntityTypeBuilder<PaymentGatewaySetting> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ApiKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.SecretKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.MerchantId)
            .HasMaxLength(200);

        builder.Property(s => s.ReturnUrl)
            .HasMaxLength(500);

        builder.Property(s => s.IpnUrl)
            .HasMaxLength(500);

        // Map AdditionalSettings as JSON
        builder.Property(s => s.AdditionalSettings)
            .HasColumnType("jsonb");

        builder.HasIndex(s => s.PaymentGatewayId)
            .IsUnique();
    }
}
