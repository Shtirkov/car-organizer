using CarOrganizer.API.Extensions;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.Obligations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarOrganizer.API.Controllers;

/// <summary>
/// The legal/financial obligations (insurance, casco, inspection, vignette, tax) of one vehicle in
/// the caller's garage. Nested under the vehicle so ownership is expressed by the route: the owner
/// comes from the token, the vehicle from the URL, and anything the caller doesn't own — vehicle or
/// obligation — is reported as <b>404, never 403</b>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/vehicles/{vehicleId:guid}/obligations")]
public class VehicleObligationsController : ControllerBase
{
    private readonly IVehicleObligationService _obligationService;

    public VehicleObligationsController(IVehicleObligationService obligationService)
    {
        _obligationService = obligationService;
    }

    [HttpGet]
    public async Task<IActionResult> List(Guid vehicleId, CancellationToken cancellationToken)
    {
        var obligations = await _obligationService.ListAsync(User.GetUserId(), vehicleId, cancellationToken);

        // null distinguishes "not your vehicle" (404) from an owned vehicle with no obligations (200, []).
        return obligations is null ? NotFound() : Ok(obligations);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        var obligation = await _obligationService.GetAsync(User.GetUserId(), vehicleId, id, cancellationToken);

        return obligation is null ? NotFound() : Ok(obligation);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid vehicleId, CreateVehicleObligationRequest request, CancellationToken cancellationToken)
    {
        var obligation = await _obligationService.CreateAsync(User.GetUserId(), vehicleId, request, cancellationToken);

        return obligation is null
            ? NotFound()
            : CreatedAtAction(nameof(Get), new { vehicleId, id = obligation.Id }, obligation);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid vehicleId, Guid id, UpdateVehicleObligationRequest request, CancellationToken cancellationToken)
    {
        var obligation = await _obligationService.UpdateAsync(User.GetUserId(), vehicleId, id, request, cancellationToken);

        return obligation is null ? NotFound() : Ok(obligation);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _obligationService.DeleteAsync(User.GetUserId(), vehicleId, id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
