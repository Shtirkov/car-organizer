using System.Reflection;
using CarOrganizer.Domain.Common;
using CarOrganizer.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarOrganizer.Infrastructure.Persistence;

/// <summary>
/// Central EF Core gateway to the database. All persistence flows through here.
/// Extends IdentityUserContext (users only, no role tables) to also own the
/// ASP.NET Identity user store.
/// </summary>
public class AppDbContext : IdentityUserContext<User, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<VehicleObligation> Obligations => Set<VehicleObligation>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        TouchTimestamps();
        return base.SaveChanges();
    }

    /// <summary>Maintains audit timestamps on added/modified entities.</summary>
    private void TouchTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
    }
}
