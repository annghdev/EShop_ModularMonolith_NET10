using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Data.Configurations;

public class ExternalAccountConfiguration : IEntityTypeConfiguration<ExternalAccount>
{
    public void Configure(EntityTypeBuilder<ExternalAccount> builder)
    {
        builder.ToTable("ExternalAccounts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ProviderKey)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.LinkedAt)
            .IsRequired();

        // Unique constraint: one provider account per user
        builder.HasIndex(e => new { e.AccountId, e.Provider })
            .IsUnique();

        // Index for finding by provider key
        builder.HasIndex(e => new { e.Provider, e.ProviderKey });
    }
}
