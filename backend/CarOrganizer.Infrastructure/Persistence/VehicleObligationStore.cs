using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarOrganizer.Infrastructure.Persistence;

/// <summary>EF Core-backed <see cref="IVehicleObligationStore"/>.</summary>
public class VehicleObligationStore : IVehicleObligationStore
{
    private readonly AppDbContext _dbContext;

    public VehicleObligationStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(VehicleObligation obligation, CancellationToken cancellationToken = default)
    {
        _dbContext.Obligations.Add(obligation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleObligation>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        // Soonest to expire first — the order the owner cares about ("what do I need to renew?").
        return await _dbContext.Obligations
            .Where(o => o.VehicleId == vehicleId)
            .OrderBy(o => o.ValidUntil)
            .ThenBy(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleObligation>> ListByOwnerDueByAsync(Guid ownerId, DateOnly dueBy, CancellationToken cancellationToken = default)
    {
        // Joins through the vehicle rather than taking a vehicleId, because the dashboard spans the
        // whole garage. ValidUntil is indexed, and the cutoff keeps far-future renewals out entirely.
        return await _dbContext.Obligations
            .Where(o => o.Vehicle.OwnerId == ownerId && o.ValidUntil <= dueBy)
            .OrderBy(o => o.ValidUntil)
            .ToListAsync(cancellationToken);
    }

    public Task<VehicleObligation?> FindByIdAsync(Guid obligationId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Obligations
            .SingleOrDefaultAsync(o => o.Id == obligationId && o.VehicleId == vehicleId, cancellationToken);
    }

    public async Task UpdateAsync(VehicleObligation obligation, CancellationToken cancellationToken = default)
    {
        _dbContext.Obligations.Update(obligation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(VehicleObligation obligation, CancellationToken cancellationToken = default)
    {
        _dbContext.Obligations.Remove(obligation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
