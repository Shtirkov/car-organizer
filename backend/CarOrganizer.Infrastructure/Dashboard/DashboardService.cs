using CarOrganizer.Application.Dashboard;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Infrastructure.Dashboard;

/// <summary>
/// Store-backed implementation of <see cref="IDashboardService"/>. Reads the owner's garage, splits
/// the obligations due inside the horizon into "already overdue" and "expiring soon", and hangs both
/// off the vehicle they belong to along with that vehicle's most recent services.
/// </summary>
/// <remarks>
/// Owns no writes and no ownership gate of its own: every lookup is already owner-scoped, so a
/// vehicle that isn't the caller's simply never appears.
/// </remarks>
public class DashboardService : IDashboardService
{
    private readonly IVehicleStore _vehicles;
    private readonly IVehicleObligationStore _obligations;
    private readonly IMaintenanceRecordStore _records;

    public DashboardService(
        IVehicleStore vehicles,
        IVehicleObligationStore obligations,
        IMaintenanceRecordStore records)
    {
        _vehicles = vehicles;
        _obligations = obligations;
        _records = records;
    }

    public async Task<DashboardResponse> GetAsync(Guid ownerId, int withinDays, int recentCount, CancellationToken cancellationToken = default)
    {
        // "Today" is the server's UTC date. For a UTC+2/+3 user this can differ from their local date
        // for a couple of hours around midnight, which is immaterial for renewals measured in weeks.
        // Revisit by taking the client's date if a day-level off-by-one ever becomes visible.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var horizon = today.AddDays(withinDays);

        var vehicles = await _vehicles.ListByOwnerAsync(ownerId, cancellationToken);

        // One query for the whole garage rather than one per vehicle; the split into the two buckets
        // is a property of each row's date, not of a separate lookup.
        var due = await _obligations.ListByOwnerDueByAsync(ownerId, horizon, cancellationToken);
        var dueByVehicle = due.ToLookup(o => o.VehicleId);

        var blocks = new List<DashboardVehicle>(vehicles.Count);

        foreach (var vehicle in vehicles)
        {
            var recent = await _records.ListRecentByVehicleAsync(vehicle.Id, recentCount, cancellationToken);
            var obligations = dueByVehicle[vehicle.Id];

            blocks.Add(new DashboardVehicle(
                vehicle.Id,
                vehicle.Make,
                vehicle.Model,
                vehicle.Year,
                vehicle.RegistrationPlate,
                vehicle.CurrentMileage,
                // The store already ordered by ValidUntil ascending, and ToLookup preserves that, so
                // the oldest expiry (the most overdue) and the nearest renewal both come out first.
                obligations.Where(o => o.ValidUntil < today).Select(o => ToOverdue(o, today)).ToArray(),
                obligations.Where(o => o.ValidUntil >= today).Select(o => ToExpiring(o, today)).ToArray(),
                recent.Select(ToRecentMaintenance).ToArray()));
        }

        return new DashboardResponse(DateTime.UtcNow, withinDays, vehicles.Count, blocks);
    }

    private static OverdueObligation ToOverdue(VehicleObligation obligation, DateOnly today) =>
        new(
            obligation.Id,
            obligation.Type,
            obligation.ValidUntil,
            today.DayNumber - obligation.ValidUntil.DayNumber,
            obligation.Provider,
            obligation.PolicyNumber);

    private static ExpiringObligation ToExpiring(VehicleObligation obligation, DateOnly today) =>
        new(
            obligation.Id,
            obligation.Type,
            obligation.ValidUntil,
            obligation.ValidUntil.DayNumber - today.DayNumber,
            obligation.Provider,
            obligation.PolicyNumber);

    private static DashboardMaintenanceRecord ToRecentMaintenance(MaintenanceRecord record) =>
        new(record.Id, record.Type, record.Date, record.Mileage, record.Cost);
}
