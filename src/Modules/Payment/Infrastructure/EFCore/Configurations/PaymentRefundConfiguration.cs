using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain;

namespace Payment.Infrastructure.EFCore.Configurations;

public class PaymentRefundConfiguration : IEntityTypeConfiguration<PaymentRefund>
{
    public void Configure(EntityTypeBuilder<PaymentRefund> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.PaymentTransactionId)
            .IsRequired();

        builder.Property(r => r.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.RefundedAt)
            .IsRequired();

        builder.Property(r => r.RefundedBy);

        builder.Property(r => r.ExternalRefundId)
            .HasMaxLength(200);

        builder.Property(r => r.CompletedAt);

        // Map RefundAmount (Money)
        builder.OwnsOne(r => r.RefundAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("RefundAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("RefundCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.HasIndex(r => r.PaymentTransactionId);

        builder.HasIndex(r => r.Status);
    }
}
