using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Persistence gateway for vehicle obligations. Implemented in the Infrastructure layer.
/// </summary>
/// <remarks>
/// Lookups are scoped by <c>vehicleId</c>: an obligation only makes sense under its vehicle, and the
/// caller has already been proven to own that vehicle, so an obligation under a different vehicle
/// simply isn't found.
/// </remarks>
public interface IVehicleObligationStore
{
    Task AddAsync(VehicleObligation obligation, CancellationToken cancellationToken = default);

    /// <summary>Every obligation for the vehicle, soonest to expire first.</summary>
    Task<IReadOnlyList<VehicleObligation>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every obligation across the owner's whole garage that is due on or before <paramref name="dueBy"/>,
    /// soonest to expire first. The dashboard's two buckets — already overdue and expiring soon — are
    /// both slices of this one result, which is why the cutoff is the only filter applied here.
    /// </summary>
    /// <remarks>
    /// The only owner-scoped lookup on this store: the dashboard spans the garage rather than sitting
    /// under one vehicle, so ownership is asserted by joining through <c>Vehicle.OwnerId</c> instead of
    /// by a <c>vehicleId</c> the caller has already been proven to own.
    /// </remarks>
    Task<IReadOnlyList<VehicleObligation>> ListByOwnerDueByAsync(Guid ownerId, DateOnly dueBy, CancellationToken cancellationToken = default);

    /// <summary>The vehicle's obligation with this id, or <c>null</c> if there is no such obligation.</summary>
    Task<VehicleObligation?> FindByIdAsync(Guid obligationId, Guid vehicleId, CancellationToken cancellationToken = default);

    Task UpdateAsync(VehicleObligation obligation, CancellationToken cancellationToken = default);

    Task RemoveAsync(VehicleObligation obligation, CancellationToken cancellationToken = default);
}
