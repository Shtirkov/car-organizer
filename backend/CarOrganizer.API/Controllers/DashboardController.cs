using System.ComponentModel.DataAnnotations;
using CarOrganizer.API.Extensions;
using CarOrganizer.Application.Dashboard;
using CarOrganizer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarOrganizer.API.Controllers;

/// <summary>
/// The caller's home screen: every vehicle in their garage with its renewals and recent services,
/// in one request. Grouped by vehicle so the client can switch between cars without going back to
/// the server.
/// </summary>
/// <remarks>
/// Read-only, and never 404: an owner with no vehicles gets an empty garage. There is no
/// <c>vehicleId</c> route — ownership comes from the token, and a vehicle that isn't the caller's is
/// simply absent from the result.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <param name="withinDays">How far ahead an obligation counts as "expiring soon".</param>
    /// <param name="recentCount">How many recent services each vehicle's block carries.</param>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery][Range(DashboardLimits.MinWithinDays, DashboardLimits.MaxWithinDays)] int withinDays = DashboardLimits.DefaultWithinDays,
        [FromQuery][Range(DashboardLimits.MinRecentCount, DashboardLimits.MaxRecentCount)] int recentCount = DashboardLimits.DefaultRecentCount,
        CancellationToken cancellationToken = default)
    {
        // Out-of-range values are turned away as 400 by [ApiController]'s automatic model validation
        // before this body runs, so the service only ever sees sane bounds.
        var dashboard = await _dashboardService.GetAsync(
            User.GetUserId(), withinDays, recentCount, cancellationToken);

        return Ok(dashboard);
    }
}
