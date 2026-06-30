namespace CarOrganizer.Domain.Enums;

/// <summary>
/// Category of a maintenance/service event performed on a vehicle.
/// </summary>
public enum MaintenanceType
{
    Other = 0,
    OilChange = 1,
    TireChange = 2,
    BrakeService = 3,
    Inspection = 4,
    Repair = 5,
    Battery = 6,
    Filters = 7,
    Suspension = 8,
}
