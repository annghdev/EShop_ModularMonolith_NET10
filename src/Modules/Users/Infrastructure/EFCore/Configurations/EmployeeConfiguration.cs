using Users.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.EFCore.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        new AggreageRootConfiguration<Employee>().Configure(builder);

        builder.Property(e => e.EmployeeCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Department)
            .HasMaxLength(100);

        builder.Property(e => e.Position)
            .HasMaxLength(100);

        builder.Property(e => e.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(e => e.AccountId)
            .IsRequired();

        builder.Property(e => e.HireDate)
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(e => e.Status)
            .IsRequired();

        #region Map ValueObjects

        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property(x => x.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(x => x.Value)
                .IsUnique();
        });

        builder.OwnsOne(e => e.Phone, phone =>
        {
            phone.Property(x => x.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20);
        });

        #endregion

        builder.Navigation(e => e.Phone)
            .IsRequired(false);

        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique();

        builder.HasIndex(e => e.AccountId)
            .IsUnique();

        builder.ToTable("Employees");
    }
}
