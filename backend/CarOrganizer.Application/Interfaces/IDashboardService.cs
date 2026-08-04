using CarOrganizer.Application.Dashboard;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Builds the caller's home screen. Implemented in the Infrastructure layer.
/// </summary>
/// <remarks>
/// Read-only and never "not found": an owner with no vehicles gets an empty garage, not a 404, so
/// this returns a response rather than a nullable one. Bounds on the two parameters are the
/// controller's business (<see cref="DashboardLimits"/>), checked before the service runs.
/// </remarks>
public interface IDashboardService
{
    /// <param name="withinDays">How far ahead an obligation counts as "expiring soon".</param>
    /// <param name="recentCount">How many recent services to include <b>per vehicle</b>.</param>
    Task<DashboardResponse> GetAsync(Guid ownerId, int withinDays, int recentCount, CancellationToken cancellationToken = default);
}
