using Users.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.EFCore.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        new AggreageRootConfiguration<Guest>().Configure(builder);

        builder.Property(g => g.GuestId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(g => g.Email)
            .HasMaxLength(255);

        builder.Property(g => g.Phone)
            .HasMaxLength(20);

        builder.Property(g => g.FullName)
            .HasMaxLength(200);

        builder.Property(g => g.FirstVisitAt)
            .IsRequired();

        builder.Property(g => g.LastActivityAt)
            .IsRequired();

        builder.Property(g => g.IsConverted)
            .IsRequired();

        builder.Property(g => g.ConvertedToCustomerId)
            .IsRequired(false);

        #region Map ValueObjects

        builder.OwnsOne(g => g.LastShippingAddress, address =>
        {
            address.Property(x => x.RecipientName)
                .HasColumnName("RecipientName")
                .HasMaxLength(200)
                .IsRequired(false);

            address.Property(x => x.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(20)
                .IsRequired(false);

            address.Property(x => x.Street)
                .HasColumnName("Street")
                .HasMaxLength(300)
                .IsRequired(false);

            address.Property(x => x.Ward)
                .HasColumnName("Ward")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(x => x.District)
                .HasColumnName("District")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(x => x.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(x => x.Country)
                .HasColumnName("Country")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(x => x.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20)
                .IsRequired(false);
        });

        #endregion

        builder.Navigation(g => g.LastShippingAddress)
            .IsRequired(false);

        builder.HasIndex(g => g.GuestId)
            .IsUnique();

        builder.HasIndex(g => g.Email);
        builder.HasIndex(g => g.IsConverted);
        builder.HasIndex(g => g.LastActivityAt);

        builder.ToTable("Guests");
    }
}
