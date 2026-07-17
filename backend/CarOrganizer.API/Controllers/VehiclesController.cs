using CarOrganizer.API.Extensions;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarOrganizer.API.Controllers;

/// <summary>
/// The caller's garage. Every action is scoped to the vehicles they own — a vehicle belonging to
/// someone else is reported as <b>404, never 403</b>, so that guessing an id can't confirm one
/// exists. The owner comes from the access token, never from the request body.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleService.ListAsync(User.GetUserId(), cancellationToken);

        return Ok(vehicles);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.GetAsync(User.GetUserId(), id, cancellationToken);

        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.CreateAsync(User.GetUserId(), request, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.UpdateAsync(User.GetUserId(), id, request, cancellationToken);

        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _vehicleService.DeleteAsync(User.GetUserId(), id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
