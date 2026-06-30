using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarOrganizer.Infrastructure.Persistence.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.HasIndex(r => r.VehicleId);
        builder.HasIndex(r => r.DueDate);

        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(40);
        builder.Property(r => r.Title).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(2000);
    }
}
