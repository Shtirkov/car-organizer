namespace CarOrganizer.Application.MaintenanceRecords;

/// <summary>
/// Bounds shared by the maintenance-record request DTOs, kept here so create and update can't drift
/// apart and so the attribute limits stay compile-time constants.
/// </summary>
public static class MaintenanceLimits
{
    /// <summary>Matches the column length in <c>MaintenanceRecordConfiguration</c>.</summary>
    public const int NotesMaxLength = 2000;

    /// <summary>A single service costing more than this is a typo, not a bill.</summary>
    public const int MaxCost = 1_000_000;
}
