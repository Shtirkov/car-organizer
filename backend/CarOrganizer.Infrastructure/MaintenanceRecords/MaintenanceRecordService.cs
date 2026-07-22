using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.MaintenanceRecords;
using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Infrastructure.MaintenanceRecords;

/// <summary>
/// Store-backed implementation of <see cref="IMaintenanceRecordService"/>. Coordinates the record
/// store with the vehicle store so that (a) every operation is scoped to a vehicle the caller owns,
/// and (b) logging a service at a higher odometer advances the vehicle's current mileage.
/// </summary>
public class MaintenanceRecordService : IMaintenanceRecordService
{
    private readonly IMaintenanceRecordStore _records;
    private readonly IVehicleStore _vehicles;

    public MaintenanceRecordService(IMaintenanceRecordStore records, IVehicleStore vehicles)
    {
        _records = records;
        _vehicles = vehicles;
    }

    public async Task<MaintenanceRecordResponse?> CreateAsync(Guid ownerId, Guid vehicleId, CreateMaintenanceRecordRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicles.FindByIdAsync(vehicleId, ownerId, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        var record = new MaintenanceRecord
        {
            VehicleId = vehicleId,
            Type = request.Type,
            Date = request.Date,
            Mileage = request.Mileage,
            Cost = request.Cost,
            Notes = request.Notes,
        };

        AdvanceCurrentMileage(vehicle, record.Mileage);

        // The vehicle just mutated above is tracked by the same context this store saves through, so
        // this single call persists both the new record and the mileage bump.
        await _records.AddAsync(record, cancellationToken);

        return ToResponse(record);
    }

    public async Task<IReadOnlyList<MaintenanceRecordResponse>?> ListAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicles.FindByIdAsync(vehicleId, ownerId, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        var records = await _records.ListByVehicleAsync(vehicleId, cancellationToken);

        return records.Select(ToResponse).ToArray();
    }

    public async Task<MaintenanceRecordResponse?> GetAsync(Guid ownerId, Guid vehicleId, Guid recordId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicles.FindByIdAsync(vehicleId, ownerId, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        var record = await _records.FindByIdAsync(recordId, vehicleId, cancellationToken);

        return record is null ? null : ToResponse(record);
    }

    public async Task<MaintenanceRecordResponse?> UpdateAsync(Guid ownerId, Guid vehicleId, Guid recordId, UpdateMaintenanceRecordRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicles.FindByIdAsync(vehicleId, ownerId, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        var record = await _records.FindByIdAsync(recordId, vehicleId, cancellationToken);
        if (record is null)
        {
            return null;
        }

        record.Type = request.Type;
        record.Date = request.Date;
        record.Mileage = request.Mileage;
        record.Cost = request.Cost;
        record.Notes = request.Notes;

        // Editing a record's mileage upward is also new odometer truth.
        AdvanceCurrentMileage(vehicle, record.Mileage);

        await _records.UpdateAsync(record, cancellationToken);

        return ToResponse(record);
    }

    public async Task<bool> DeleteAsync(Guid ownerId, Guid vehicleId, Guid recordId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicles.FindByIdAsync(vehicleId, ownerId, cancellationToken);
        if (vehicle is null)
        {
            return false;
        }

        var record = await _records.FindByIdAsync(recordId, vehicleId, cancellationToken);
        if (record is null)
        {
            return false;
        }

        await _records.RemoveAsync(record, cancellationToken);

        // CurrentMileage is a high-water mark: the car really did reach that reading, so deleting the
        // record that set it does not pull it back down. A mistaken reading is corrected with a
        // manual vehicle update, not by deletion.
        return true;
    }

    private static void AdvanceCurrentMileage(Vehicle vehicle, int recordMileage)
    {
        if (recordMileage > vehicle.CurrentMileage)
        {
            vehicle.CurrentMileage = recordMileage;
        }
    }

    private static MaintenanceRecordResponse ToResponse(MaintenanceRecord record) =>
        new(
            record.Id,
            record.Type,
            record.Date,
            record.Mileage,
            record.Cost,
            record.Notes,
            record.CreatedAtUtc,
            record.UpdatedAtUtc);
}
