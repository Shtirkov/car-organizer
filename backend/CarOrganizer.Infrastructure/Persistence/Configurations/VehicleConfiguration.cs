using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarOrganizer.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasIndex(v => v.OwnerId);

        builder.Property(v => v.Make).HasMaxLength(80).IsRequired();
        builder.Property(v => v.Model).HasMaxLength(80).IsRequired();
        builder.Property(v => v.Vin).HasMaxLength(17);
        builder.Property(v => v.RegistrationPlate).HasMaxLength(20);
        builder.Property(v => v.Engine).HasMaxLength(80);

        builder.HasMany(v => v.MaintenanceRecords)
            .WithOne(m => m.Vehicle)
            .HasForeignKey(m => m.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Documents)
            .WithOne(d => d.Vehicle)
            .HasForeignKey(d => d.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Reminders)
            .WithOne(r => r.Vehicle)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
