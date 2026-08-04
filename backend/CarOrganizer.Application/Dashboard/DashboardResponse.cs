using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Application.Dashboard;

/// <summary>
/// The whole home screen in one payload: every vehicle in the caller's garage, each carrying its own
/// renewals and recent services. <c>WithinDays</c> echoes the horizon actually applied, so the client
/// needn't assume the default.
/// </summary>
/// <remarks>
/// Grouped by vehicle rather than flattened across the garage, because the client shows one selected
/// vehicle at a time and switching between them should not need another request.
/// </remarks>
public record DashboardResponse(
    DateTime GeneratedAtUtc,
    int WithinDays,
    int VehicleCount,
    IReadOnlyList<DashboardVehicle> Vehicles);

/// <summary>
/// One vehicle's block on the dashboard. <c>OverdueObligations</c> sits at the top of the block
/// (most overdue first) and <c>ExpiringObligations</c> below it (soonest first), so the most urgent
/// thing is always the first thing read.
/// </summary>
public record DashboardVehicle(
    Guid Id,
    string Make,
    string Model,
    int Year,
    string? RegistrationPlate,
    int CurrentMileage,
    IReadOnlyList<OverdueObligation> OverdueObligations,
    IReadOnlyList<ExpiringObligation> ExpiringObligations,
    IReadOnlyList<DashboardMaintenanceRecord> RecentMaintenance);

/// <summary>
/// An obligation whose renewal date has already passed. <c>DaysOverdue</c> counts whole days since
/// <c>ValidUntil</c> and is always at least 1.
/// </summary>
public record OverdueObligation(
    Guid Id,
    ObligationType Type,
    DateOnly ValidUntil,
    int DaysOverdue,
    string? Provider,
    string? PolicyNumber);

/// <summary>
/// An obligation coming due inside the requested horizon. <c>DaysRemaining</c> counts whole days
/// until <c>ValidUntil</c>; <c>0</c> means it expires today.
/// </summary>
/// <remarks>
/// A separate record from <see cref="OverdueObligation"/> rather than one type with a signed day
/// count: the two are different things on the screen, and each carries only the number that applies.
/// </remarks>
public record ExpiringObligation(
    Guid Id,
    ObligationType Type,
    DateOnly ValidUntil,
    int DaysRemaining,
    string? Provider,
    string? PolicyNumber);

/// <summary>A recent service, trimmed to what a summary row needs.</summary>
public record DashboardMaintenanceRecord(
    Guid Id,
    MaintenanceType Type,
    DateOnly Date,
    int Mileage,
    decimal Cost);
