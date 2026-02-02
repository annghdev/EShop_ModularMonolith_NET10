using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain;

namespace Payment.Infrastructure.EFCore.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        new AggreageRootConfiguration<PaymentTransaction>().Configure(builder);

        builder.Property(p => p.TransactionNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.CustomerId)
            .IsRequired();

        builder.Property(p => p.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.InitiatedAt)
            .IsRequired();

        builder.Property(p => p.CompletedAt);

        builder.Property(p => p.ExpiresAt);

        #region Map ValueObjects

        // Map Amount (Money)
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        // Map PaymentMetadata
        builder.OwnsOne(p => p.Metadata, meta =>
        {
            meta.Property(m => m.ExternalTransactionId)
                .HasColumnName("ExternalTransactionId")
                .HasMaxLength(200);

            meta.Property(m => m.BankCode)
                .HasColumnName("BankCode")
                .HasMaxLength(50);

            meta.Property(m => m.CardType)
                .HasColumnName("CardType")
                .HasMaxLength(50);

            meta.Property(m => m.ResponseCode)
                .HasColumnName("ResponseCode")
                .HasMaxLength(50);

            meta.Property(m => m.ResponseMessage)
                .HasColumnName("ResponseMessage")
                .HasMaxLength(500);

            meta.Property(m => m.IpAddress)
                .HasColumnName("IpAddress")
                .HasMaxLength(50);

            meta.Property(m => m.AdditionalData)
                .HasColumnName("AdditionalData")
                .HasColumnType("jsonb");
        });

        #endregion

        #region Relationships

        // Many-to-one with PaymentGateway
        builder.HasOne(p => p.Gateway)
            .WithMany()
            .HasForeignKey(p => p.GatewayId)
            .OnDelete(DeleteBehavior.SetNull);

        // One-to-many with PaymentRefund using backing field
        builder.HasMany(p => p.Refunds)
            .WithOne(r => r.PaymentTransaction)
            .HasForeignKey(r => r.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(PaymentTransaction.Refunds))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        #endregion

        #region Indexes

        builder.HasIndex(p => p.TransactionNumber)
            .IsUnique();

        builder.HasIndex(p => p.OrderId);

        builder.HasIndex(p => p.OrderNumber);

        builder.HasIndex(p => p.CustomerId);

        builder.HasIndex(p => p.Status);

        builder.HasIndex(p => p.InitiatedAt);

        #endregion
    }
}
