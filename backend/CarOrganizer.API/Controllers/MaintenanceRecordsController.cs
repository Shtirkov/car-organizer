using CarOrganizer.API.Extensions;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.MaintenanceRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarOrganizer.API.Controllers;

/// <summary>
/// The service history of one vehicle in the caller's garage. Nested under the vehicle so ownership
/// is expressed by the route: the owner comes from the token, the vehicle from the URL, and anything
/// the caller doesn't own — vehicle or record — is reported as <b>404, never 403</b>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/vehicles/{vehicleId:guid}/maintenance-records")]
public class MaintenanceRecordsController : ControllerBase
{
    private readonly IMaintenanceRecordService _maintenanceRecordService;

    public MaintenanceRecordsController(IMaintenanceRecordService maintenanceRecordService)
    {
        _maintenanceRecordService = maintenanceRecordService;
    }

    [HttpGet]
    public async Task<IActionResult> List(Guid vehicleId, CancellationToken cancellationToken)
    {
        var records = await _maintenanceRecordService.ListAsync(User.GetUserId(), vehicleId, cancellationToken);

        // null distinguishes "not your vehicle" (404) from an owned vehicle with no records (200, []).
        return records is null ? NotFound() : Ok(records);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        var record = await _maintenanceRecordService.GetAsync(User.GetUserId(), vehicleId, id, cancellationToken);

        return record is null ? NotFound() : Ok(record);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid vehicleId, CreateMaintenanceRecordRequest request, CancellationToken cancellationToken)
    {
        var record = await _maintenanceRecordService.CreateAsync(User.GetUserId(), vehicleId, request, cancellationToken);

        return record is null
            ? NotFound()
            : CreatedAtAction(nameof(Get), new { vehicleId, id = record.Id }, record);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid vehicleId, Guid id, UpdateMaintenanceRecordRequest request, CancellationToken cancellationToken)
    {
        var record = await _maintenanceRecordService.UpdateAsync(User.GetUserId(), vehicleId, id, request, cancellationToken);

        return record is null ? NotFound() : Ok(record);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _maintenanceRecordService.DeleteAsync(User.GetUserId(), vehicleId, id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
