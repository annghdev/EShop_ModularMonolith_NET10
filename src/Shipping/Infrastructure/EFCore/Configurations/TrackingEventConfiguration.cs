using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Domain;

namespace Shipping.Infrastructure.EFCore.Configurations;

public class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ShipmentId)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Location)
            .HasMaxLength(200);

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.Property(e => e.ExternalEventId)
            .HasMaxLength(200);

        builder.Property(e => e.RawData)
            .HasColumnType("text");

        builder.HasIndex(e => e.ShipmentId);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.Timestamp);
    }
}
