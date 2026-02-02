using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain;

namespace Payment.Infrastructure.EFCore.Configurations;

public class PaymentGatewayConfiguration : IEntityTypeConfiguration<PaymentGateway>
{
    public void Configure(EntityTypeBuilder<PaymentGateway> builder)
    {
        new AggreageRootConfiguration<PaymentGateway>().Configure(builder);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.IsEnabled)
            .IsRequired();

        builder.Property(p => p.DisplayOrder)
            .IsRequired();

        // Map SupportedCurrencies as JSON (backing field)
        builder.Property<List<string>>("_supportedCurrencies")
            .HasColumnName("SupportedCurrencies")
            .HasColumnType("jsonb");

        builder.Ignore(p => p.SupportedCurrencies);

        // Configure one-to-one relationship with PaymentGatewaySetting
        builder.HasOne(p => p.Setting)
            .WithOne(s => s.PaymentGateway)
            .HasForeignKey<PaymentGatewaySetting>(s => s.PaymentGatewayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasIndex(p => p.Provider);
    }
}
