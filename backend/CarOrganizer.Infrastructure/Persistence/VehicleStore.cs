using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarOrganizer.Infrastructure.Persistence;

/// <summary>EF Core-backed <see cref="IVehicleStore"/>.</summary>
public class VehicleStore : IVehicleStore
{
    private readonly AppDbContext _dbContext;

    public VehicleStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        _dbContext.Vehicles.Add(vehicle);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles
            .Where(v => v.OwnerId == ownerId)
            .OrderByDescending(v => v.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Vehicle?> FindByIdAsync(Guid vehicleId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        // The OwnerId predicate is what makes this safe: a vehicle belonging to someone else simply
        // isn't found, so callers can't act on it by guessing an id.
        return _dbContext.Vehicles
            .SingleOrDefaultAsync(v => v.Id == vehicleId && v.OwnerId == ownerId, cancellationToken);
    }

    public async Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        _dbContext.Vehicles.Update(vehicle);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        _dbContext.Vehicles.Remove(vehicle);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
