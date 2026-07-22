using System.ComponentModel.DataAnnotations;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Application.MaintenanceRecords;

/// <summary>
/// Payload for editing a maintenance record. A full replacement (PUT): every field is written, so
/// omitting the optional <c>Notes</c> clears it.
/// </summary>
public record UpdateMaintenanceRecordRequest(
    [EnumDataType(typeof(MaintenanceType))] MaintenanceType Type,
    DateOnly Date,
    [Range(0, VehicleLimits.MaxMileage)] int Mileage,
    [Range(0, MaintenanceLimits.MaxCost)] decimal Cost,
    [MaxLength(MaintenanceLimits.NotesMaxLength)] string? Notes);
