using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarOrganizer.Infrastructure.Persistence;

/// <summary>EF Core-backed <see cref="IMaintenanceRecordStore"/>.</summary>
public class MaintenanceRecordStore : IMaintenanceRecordStore
{
    private readonly AppDbContext _dbContext;

    public MaintenanceRecordStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MaintenanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.MaintenanceRecords.Add(record);
        // One SaveChanges also flushes any other change tracked on this shared context — notably the
        // owning vehicle's advanced CurrentMileage — so the record and the bump commit together.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MaintenanceRecord>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MaintenanceRecords
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<MaintenanceRecord?> FindByIdAsync(Guid recordId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        // Scoping by VehicleId is what keeps a record reachable only through the vehicle the caller
        // has already been proven to own.
        return _dbContext.MaintenanceRecords
            .SingleOrDefaultAsync(r => r.Id == recordId && r.VehicleId == vehicleId, cancellationToken);
    }

    public async Task UpdateAsync(MaintenanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.MaintenanceRecords.Update(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(MaintenanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.MaintenanceRecords.Remove(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
