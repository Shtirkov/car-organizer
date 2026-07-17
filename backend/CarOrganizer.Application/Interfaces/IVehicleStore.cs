using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Persistence gateway for vehicles. Implemented in the Infrastructure layer.
/// </summary>
/// <remarks>
/// Note that the lookups take an owner id rather than exposing a plain "find by id": ownership is
/// part of the question being asked, so a caller cannot forget to check it and hand back someone
/// else's vehicle.
/// </remarks>
public interface IVehicleStore
{
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

    /// <summary>Every vehicle in the owner's garage, newest first.</summary>
    Task<IReadOnlyList<Vehicle>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>The owner's vehicle with this id, or <c>null</c> if they have no such vehicle.</summary>
    Task<Vehicle?> FindByIdAsync(Guid vehicleId, Guid ownerId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

    Task RemoveAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
}
