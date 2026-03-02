using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class MeasurementEntityConfiguration : IEntityTypeConfiguration<MeasurementEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementEntity> builder)
    {
        builder.ToTable("measurements");

        builder.HasKey(x => new { x.AquariumId, x.Timestamp });

        builder.Property(x => x.AquariumId).HasColumnName("aquarium_id").HasColumnType("text");
        builder.Property(x => x.Timestamp).HasColumnName("timestamp").HasColumnType("timestamptz");
        builder.Property(x => x.Temperature).HasColumnName("temperature").HasColumnType("numeric");
        builder.Property(x => x.Mg).HasColumnName("mg").HasColumnType("numeric");
        builder.Property(x => x.Kh).HasColumnName("kh").HasColumnType("numeric");
        builder.Property(x => x.Ca).HasColumnName("ca").HasColumnType("numeric");
        builder.Property(x => x.Ph).HasColumnName("ph").HasColumnType("numeric");
        builder.Property(x => x.Oxygen).HasColumnName("oxygen").HasColumnType("numeric");
        builder.Property(x => x.Pump).HasColumnName("pump").HasColumnType("numeric");

        builder.HasIndex(x => x.Timestamp).HasDatabaseName("ix_measurements_timestamp");
        builder.HasIndex(x => new { x.AquariumId, x.Timestamp })
            .IsDescending(false, true)
            .HasDatabaseName("ix_measurements_aquarium_timestamp_desc");
    }
}
