using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Infrastructure.Vehicles;

/// <summary>
/// Store-backed implementation of <see cref="IVehicleService"/>. Owns the mapping between the
/// <see cref="Vehicle"/> entity and the DTOs, and threads the owner id into every lookup so a
/// caller only ever reaches their own garage.
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly IVehicleStore _vehicleStore;
    private readonly IDocumentStore _documents;
    private readonly IFileStorage _storage;

    public VehicleService(IVehicleStore vehicleStore, IDocumentStore documents, IFileStorage storage)
    {
        _vehicleStore = vehicleStore;
        _documents = documents;
        _storage = storage;
    }

    public async Task<VehicleResponse> CreateAsync(Guid ownerId, CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = new Vehicle
        {
            OwnerId = ownerId,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            PurchaseMileage = request.PurchaseMileage,
            // Omitted current reading means "same as purchase" — a car only just acquired.
            CurrentMileage = request.CurrentMileage ?? request.PurchaseMileage,
            Vin = request.Vin,
            RegistrationPlate = request.RegistrationPlate,
            Engine = request.Engine,
        };

        await _vehicleStore.AddAsync(vehicle, cancellationToken);

        return ToResponse(vehicle);
    }

    public async Task<IReadOnlyList<VehicleResponse>> ListAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleStore.ListByOwnerAsync(ownerId, cancellationToken);

        return vehicles.Select(ToResponse).ToArray();
    }

    public async Task<VehicleResponse?> GetAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleStore.FindByIdAsync(vehicleId, ownerId, cancellationToken);

        return vehicle is null ? null : ToResponse(vehicle);
    }

    public async Task<VehicleResponse?> UpdateAsync(Guid ownerId, Guid vehicleId, UpdateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleStore.FindByIdAsync(vehicleId, ownerId, cancellationToken);

        if (vehicle is null)
        {
            return null;
        }

        // A PUT replaces the whole resource, so every editable field is written — including the
        // optional ones, which a request that omits them is asking to clear. OwnerId is not
        // editable: a vehicle cannot change hands.
        vehicle.Make = request.Make;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.PurchaseMileage = request.PurchaseMileage;
        vehicle.CurrentMileage = request.CurrentMileage;
        vehicle.Vin = request.Vin;
        vehicle.RegistrationPlate = request.RegistrationPlate;
        vehicle.Engine = request.Engine;

        await _vehicleStore.UpdateAsync(vehicle, cancellationToken);

        return ToResponse(vehicle);
    }

    public async Task<bool> DeleteAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleStore.FindByIdAsync(vehicleId, ownerId, cancellationToken);

        if (vehicle is null)
        {
            return false;
        }

        // Scrapping the car takes its whole paper trail with it — rows by cascade, files by us.
        var documents = await _documents.ListByVehicleAsync(vehicleId, cancellationToken);

        await _vehicleStore.RemoveAsync(vehicle, cancellationToken);

        foreach (var document in documents)
        {
            await _storage.DeleteAsync(document.StorageKey, cancellationToken);
        }

        return true;
    }

    private static VehicleResponse ToResponse(Vehicle vehicle) =>
        new(
            vehicle.Id,
            vehicle.Make,
            vehicle.Model,
            vehicle.Year,
            vehicle.PurchaseMileage,
            vehicle.CurrentMileage,
            vehicle.Vin,
            vehicle.RegistrationPlate,
            vehicle.Engine,
            vehicle.CreatedAtUtc,
            vehicle.UpdatedAtUtc);
}
