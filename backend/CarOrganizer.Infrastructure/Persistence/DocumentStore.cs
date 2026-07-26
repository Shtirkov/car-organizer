using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarOrganizer.Infrastructure.Persistence;

/// <summary>EF Core-backed <see cref="IDocumentStore"/>.</summary>
public class DocumentStore : IDocumentStore
{
    private readonly AppDbContext _dbContext;

    public DocumentStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        // Newest first — the document you just photographed is the one you're looking for.
        return await _dbContext.Documents
            .Where(d => d.VehicleId == vehicleId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Document?> FindByIdAsync(Guid documentId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        // Scoping by VehicleId is what keeps a document reachable only through the vehicle the caller
        // has already been proven to own.
        return _dbContext.Documents
            .SingleOrDefaultAsync(d => d.Id == documentId && d.VehicleId == vehicleId, cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> ListByMaintenanceRecordAsync(Guid maintenanceRecordId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .Where(d => d.MaintenanceRecordId == maintenanceRecordId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> ListByObligationAsync(Guid obligationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .Where(d => d.VehicleObligationId == obligationId)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(Document document, CancellationToken cancellationToken = default)
    {
        _dbContext.Documents.Remove(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
