using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Persistence gateway for maintenance records. Implemented in the Infrastructure layer.
/// </summary>
/// <remarks>
/// Lookups are scoped by <c>vehicleId</c> rather than exposing a bare "find by id": a record only
/// ever makes sense in the context of its vehicle, and the caller has already proven ownership of
/// that vehicle, so a record under a different vehicle simply isn't found.
/// </remarks>
public interface IMaintenanceRecordStore
{
    Task AddAsync(MaintenanceRecord record, CancellationToken cancellationToken = default);

    /// <summary>Every record for the vehicle, most recent service first.</summary>
    Task<IReadOnlyList<MaintenanceRecord>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>The vehicle's record with this id, or <c>null</c> if there is no such record.</summary>
    Task<MaintenanceRecord?> FindByIdAsync(Guid recordId, Guid vehicleId, CancellationToken cancellationToken = default);

    Task UpdateAsync(MaintenanceRecord record, CancellationToken cancellationToken = default);

    Task RemoveAsync(MaintenanceRecord record, CancellationToken cancellationToken = default);
}
