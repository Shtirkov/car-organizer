using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Application.MaintenanceRecords;

/// <summary>
/// A maintenance record as returned to the owner. Carries no <c>VehicleId</c> — the vehicle is
/// already fixed by the URL the record was reached through.
/// </summary>
public record MaintenanceRecordResponse(
    Guid Id,
    MaintenanceType Type,
    DateOnly Date,
    int Mileage,
    decimal Cost,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
