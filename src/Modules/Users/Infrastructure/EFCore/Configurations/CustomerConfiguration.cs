using Users.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.EFCore.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        new AggreageRootConfiguration<Customer>().Configure(builder);

        builder.Property(c => c.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(c => c.AccountId)
            .IsRequired(false);

        builder.Property(c => c.LoyaltyPoints)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(c => c.DateOfBirth)
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(c => c.Gender)
            .IsRequired(false);

        #region Map ValueObjects

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(e => e.Value)
                .IsUnique();
        });

        builder.OwnsOne(c => c.Phone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20);
        });

        #endregion

        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Customer.Addresses))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(c => c.Phone)
            .IsRequired(false);

        builder.ToTable("Customers");
    }
}

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.IsDefault)
            .IsRequired();

        builder.Property(a => a.Label)
            .HasMaxLength(100);

        #region Map ValueObjects

        builder.OwnsOne(a => a.Address, address =>
        {
            address.Property(x => x.RecipientName)
                .HasColumnName("RecipientName")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(x => x.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(x => x.Street)
                .HasColumnName("Street")
                .HasMaxLength(300)
                .IsRequired();

            address.Property(x => x.Ward)
                .HasColumnName("Ward")
                .HasMaxLength(100);

            address.Property(x => x.District)
                .HasColumnName("District")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(x => x.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(x => x.Country)
                .HasColumnName("Country")
                .HasMaxLength(100);

            address.Property(x => x.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20);
        });

        #endregion

        builder.HasIndex(a => a.CustomerId);

        builder.ToTable("CustomerAddresses");
    }
}
