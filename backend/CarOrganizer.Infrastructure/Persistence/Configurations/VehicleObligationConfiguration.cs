using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarOrganizer.Infrastructure.Persistence.Configurations;

public class VehicleObligationConfiguration : IEntityTypeConfiguration<VehicleObligation>
{
    public void Configure(EntityTypeBuilder<VehicleObligation> builder)
    {
        builder.HasIndex(o => o.VehicleId);

        // Indexed because the dashboard (Phase 6) will query obligations by how soon they expire.
        builder.HasIndex(o => o.ValidUntil);

        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(40);
        builder.Property(o => o.Cost).HasPrecision(18, 2);
        builder.Property(o => o.Provider).HasMaxLength(120);
        builder.Property(o => o.PolicyNumber).HasMaxLength(80);
        builder.Property(o => o.Notes).HasMaxLength(2000);
    }
}
