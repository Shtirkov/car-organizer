using CarOrganizer.Application.Vehicles;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Garage operations, all scoped to a single owner. Implemented in the Infrastructure layer.
/// </summary>
/// <remarks>
/// These return plain values rather than <see cref="Common.Result"/> (as <see cref="IAuthService"/>
/// does) because vehicle CRUD has no failure to describe: shape errors are rejected by model
/// validation before the service is reached, and the only other outcome is "the owner has no such
/// vehicle" — which <c>null</c> already says. Wrapping that in an always-empty error list would be
/// ceremony. Reach for <c>Result</c> here the day a real domain rule can fail.
/// </remarks>
public interface IVehicleService
{
    Task<VehicleResponse> CreateAsync(Guid ownerId, CreateVehicleRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VehicleResponse>> ListAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>The owner's vehicle, or <c>null</c> if it doesn't exist or isn't theirs.</summary>
    Task<VehicleResponse?> GetAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>The updated vehicle, or <c>null</c> if it doesn't exist or isn't the owner's.</summary>
    Task<VehicleResponse?> UpdateAsync(Guid ownerId, Guid vehicleId, UpdateVehicleRequest request, CancellationToken cancellationToken = default);

    /// <summary><c>true</c> if a vehicle was deleted; <c>false</c> if the owner had no such vehicle.</summary>
    Task<bool> DeleteAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default);
}
