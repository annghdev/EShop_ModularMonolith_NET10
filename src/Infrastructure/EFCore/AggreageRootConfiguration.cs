using Kernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure;

public class AggreageRootConfiguration<TAggreateRoot> : IEntityTypeConfiguration<TAggreateRoot>
    where TAggreateRoot : AggregateRoot
{
    public virtual void Configure(EntityTypeBuilder<TAggreateRoot> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Ignore(e => e.Events);

        builder.Property(e => e.Version)
               .IsConcurrencyToken()
               .IsRequired();
    }
}
