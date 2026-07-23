using CarOrganizer.Application.Obligations;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Obligation operations for a single vehicle in a single owner's garage. Implemented in the
/// Infrastructure layer.
/// </summary>
/// <remarks>
/// Every method first proves the vehicle is the caller's, so a <c>null</c> (or <c>false</c>) return
/// covers both "no such vehicle / not yours" and "no such obligation" — indistinguishable by design
/// (the 404-not-403 rule). Plain values rather than <see cref="Common.Result"/>: there is no domain
/// failure to carry beyond the shape errors model validation already rejects.
/// </remarks>
public interface IVehicleObligationService
{
    /// <summary>The created obligation, or <c>null</c> if the vehicle doesn't exist or isn't the owner's.</summary>
    Task<VehicleObligationResponse?> CreateAsync(Guid ownerId, Guid vehicleId, CreateVehicleObligationRequest request, CancellationToken cancellationToken = default);

    /// <summary>The vehicle's obligations, or <c>null</c> if the vehicle doesn't exist or isn't the owner's.</summary>
    Task<IReadOnlyList<VehicleObligationResponse>?> ListAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>The obligation, or <c>null</c> if the vehicle/obligation doesn't exist or isn't the owner's.</summary>
    Task<VehicleObligationResponse?> GetAsync(Guid ownerId, Guid vehicleId, Guid obligationId, CancellationToken cancellationToken = default);

    /// <summary>The updated obligation, or <c>null</c> if the vehicle/obligation doesn't exist or isn't the owner's.</summary>
    Task<VehicleObligationResponse?> UpdateAsync(Guid ownerId, Guid vehicleId, Guid obligationId, UpdateVehicleObligationRequest request, CancellationToken cancellationToken = default);

    /// <summary><c>true</c> if an obligation was deleted; <c>false</c> if the vehicle/obligation wasn't found or isn't the owner's.</summary>
    Task<bool> DeleteAsync(Guid ownerId, Guid vehicleId, Guid obligationId, CancellationToken cancellationToken = default);
}
