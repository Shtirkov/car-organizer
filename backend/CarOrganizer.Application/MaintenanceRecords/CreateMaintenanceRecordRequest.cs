using System.ComponentModel.DataAnnotations;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Application.MaintenanceRecords;

/// <summary>Payload for logging a service performed on one of the caller's vehicles.</summary>
/// <remarks>
/// The mileage bound is shared with <see cref="VehicleLimits.MaxMileage"/> — a record's mileage is
/// the same odometer as the vehicle's, so the two must agree on what counts as plausible.
/// </remarks>
public record CreateMaintenanceRecordRequest(
    [EnumDataType(typeof(MaintenanceType))] MaintenanceType Type,
    DateOnly Date,
    [Range(0, VehicleLimits.MaxMileage)] int Mileage,
    [Range(0, MaintenanceLimits.MaxCost)] decimal Cost,
    [MaxLength(MaintenanceLimits.NotesMaxLength)] string? Notes);
