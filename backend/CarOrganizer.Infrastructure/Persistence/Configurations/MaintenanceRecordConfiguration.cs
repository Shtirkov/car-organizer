using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarOrganizer.Infrastructure.Persistence.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.HasIndex(m => m.VehicleId);

        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(40);
        builder.Property(m => m.Cost).HasPrecision(18, 2);
        builder.Property(m => m.Notes).HasMaxLength(2000);

        builder.HasMany(m => m.Documents)
            .WithOne(d => d.MaintenanceRecord)
            .HasForeignKey(d => d.MaintenanceRecordId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
