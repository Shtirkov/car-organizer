using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarOrganizer.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasIndex(d => d.VehicleId);
        builder.HasIndex(d => d.MaintenanceRecordId);
        builder.HasIndex(d => d.VehicleObligationId);

        builder.Property(d => d.FileName).HasMaxLength(255).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(d => d.StorageKey).HasMaxLength(512).IsRequired();

        // Mirrors the maintenance-record link (configured from MaintenanceRecordConfiguration):
        // cascade, because a detached document is paperwork nobody can name the purpose of. Declared
        // with WithMany() and no back-collection so VehicleObligation stays unaware of documents.
        builder.HasOne(d => d.VehicleObligation)
            .WithMany()
            .HasForeignKey(d => d.VehicleObligationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
