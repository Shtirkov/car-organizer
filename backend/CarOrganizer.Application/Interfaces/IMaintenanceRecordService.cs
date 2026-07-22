using CarOrganizer.Application.MaintenanceRecords;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Maintenance-record operations for a single vehicle in a single owner's garage. Implemented in
/// the Infrastructure layer.
/// </summary>
/// <remarks>
/// Every method first proves the vehicle is the caller's, so a <c>null</c> (or <c>false</c>) return
/// covers both "no such vehicle / not yours" and "no such record" — the caller can't tell the two
/// apart, which is the point (see the 404-not-403 rule). Plain values rather than
/// <see cref="Common.Result"/>: mileage auto-advance can't fail, so there is no error to carry.
/// </remarks>
public interface IMaintenanceRecordService
{
    /// <summary>The created record, or <c>null</c> if the vehicle doesn't exist or isn't the owner's.</summary>
    Task<MaintenanceRecordResponse?> CreateAsync(Guid ownerId, Guid vehicleId, CreateMaintenanceRecordRequest request, CancellationToken cancellationToken = default);

    /// <summary>The vehicle's records, or <c>null</c> if the vehicle doesn't exist or isn't the owner's.</summary>
    Task<IReadOnlyList<MaintenanceRecordResponse>?> ListAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>The record, or <c>null</c> if the vehicle/record doesn't exist or isn't the owner's.</summary>
    Task<MaintenanceRecordResponse?> GetAsync(Guid ownerId, Guid vehicleId, Guid recordId, CancellationToken cancellationToken = default);

    /// <summary>The updated record, or <c>null</c> if the vehicle/record doesn't exist or isn't the owner's.</summary>
    Task<MaintenanceRecordResponse?> UpdateAsync(Guid ownerId, Guid vehicleId, Guid recordId, UpdateMaintenanceRecordRequest request, CancellationToken cancellationToken = default);

    /// <summary><c>true</c> if a record was deleted; <c>false</c> if the vehicle/record wasn't found or isn't the owner's.</summary>
    Task<bool> DeleteAsync(Guid ownerId, Guid vehicleId, Guid recordId, CancellationToken cancellationToken = default);
}
