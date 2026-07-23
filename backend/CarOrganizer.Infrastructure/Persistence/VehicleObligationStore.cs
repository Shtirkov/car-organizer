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
        return await _dbContext.Obligations
            .Where(o => o.VehicleId == vehicleId)
            .OrderByDescending(o => o.ValidUntil)
            .ThenByDescending(o => o.CreatedAtUtc)
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
