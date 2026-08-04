namespace CarOrganizer.Application.Dashboard;

/// <summary>
/// Bounds and defaults for the dashboard's query parameters, kept in one place so the controller's
/// attributes, the service and the tests can't drift apart.
/// </summary>
public static class DashboardLimits
{
    /// <summary>How far ahead "expiring soon" looks when the caller doesn't say.</summary>
    public const int DefaultWithinDays = 30;

    public const int MinWithinDays = 1;

    /// <summary>A year ahead is the widest useful horizon — obligations renew annually at most.</summary>
    public const int MaxWithinDays = 365;

    /// <summary>How many recent services each vehicle's block carries when the caller doesn't say.</summary>
    public const int DefaultRecentCount = 5;

    public const int MinRecentCount = 1;

    /// <summary>A dashboard is a summary; anything longer belongs on the vehicle's own history screen.</summary>
    public const int MaxRecentCount = 50;
}
