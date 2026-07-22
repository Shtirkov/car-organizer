using System.Security.Claims;
using CarOrganizer.API.Controllers;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.MaintenanceRecords;
using CarOrganizer.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarOrganizer.UnitTests.MaintenanceRecords;

/// <summary>
/// Covers <see cref="MaintenanceRecordsController"/>: mapping the service's outcome to the right HTTP
/// result, and passing the owner (from the token) and vehicle (from the route) into the service.
/// </summary>
public class MaintenanceRecordsControllerTests
{
    private static readonly Guid CallerId = Guid.NewGuid();
    private static readonly Guid VehicleId = Guid.NewGuid();

    private readonly Mock<IMaintenanceRecordService> _service = new();
    private readonly MaintenanceRecordsController _sut;

    public MaintenanceRecordsControllerTests()
    {
        _sut = new MaintenanceRecordsController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", CallerId.ToString())], "Test")),
                },
            },
        };
    }

    private static MaintenanceRecordResponse SampleResponse(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), MaintenanceType.OilChange, new DateOnly(2026, 6, 1), 195_000, 120m, null, DateTime.UtcNow, null);

    private static CreateMaintenanceRecordRequest CreateRequest() =>
        new(MaintenanceType.OilChange, new DateOnly(2026, 6, 1), 195_000, 120m, null);

    private static UpdateMaintenanceRecordRequest UpdateRequest() =>
        new(MaintenanceType.BrakeService, new DateOnly(2026, 7, 1), 205_000, 300m, null);

    [Fact]
    public async Task List_WhenServiceReturnsRecords_ReturnsOk()
    {
        _service
            .Setup(s => s.ListAsync(CallerId, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([SampleResponse()]);

        var response = await _sut.List(VehicleId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        var records = Assert.IsAssignableFrom<IReadOnlyList<MaintenanceRecordResponse>>(ok.Value);
        Assert.Single(records);
    }

    [Fact]
    public async Task List_WhenVehicleNotFound_ReturnsNotFound()
    {
        _service
            .Setup(s => s.ListAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<MaintenanceRecordResponse>?)null);

        var response = await _sut.List(VehicleId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Get_WhenFound_ReturnsOk()
    {
        var record = SampleResponse();
        _service
            .Setup(s => s.GetAsync(CallerId, VehicleId, record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var response = await _sut.Get(VehicleId, record.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(record, ok.Value);
    }

    [Fact]
    public async Task Get_WhenMissing_ReturnsNotFound()
    {
        _service
            .Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecordResponse?)null);

        var response = await _sut.Get(VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Create_WhenCreated_ReturnsCreatedAtTheRecordsUrlWithBothRouteValues()
    {
        var record = SampleResponse();
        _service
            .Setup(s => s.CreateAsync(CallerId, VehicleId, It.IsAny<CreateMaintenanceRecordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var response = await _sut.Create(VehicleId, CreateRequest(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(nameof(MaintenanceRecordsController.Get), created.ActionName);
        Assert.Equal(VehicleId, created.RouteValues!["vehicleId"]);
        Assert.Equal(record.Id, created.RouteValues["id"]);
        Assert.Same(record, created.Value);
    }

    [Fact]
    public async Task Create_UsesTheOwnerFromTheTokenAndVehicleFromTheRoute()
    {
        var request = CreateRequest();
        _service
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateMaintenanceRecordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        await _sut.Create(VehicleId, request, CancellationToken.None);

        _service.Verify(s => s.CreateAsync(CallerId, VehicleId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenVehicleNotFound_ReturnsNotFound()
    {
        _service
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateMaintenanceRecordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecordResponse?)null);

        var response = await _sut.Create(VehicleId, CreateRequest(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Update_WhenUpdated_ReturnsOk()
    {
        var record = SampleResponse();
        _service
            .Setup(s => s.UpdateAsync(CallerId, VehicleId, record.Id, It.IsAny<UpdateMaintenanceRecordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var response = await _sut.Update(VehicleId, record.Id, UpdateRequest(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(record, ok.Value);
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        _service
            .Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateMaintenanceRecordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecordResponse?)null);

        var response = await _sut.Update(VehicleId, Guid.NewGuid(), UpdateRequest(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Delete_WhenDeleted_ReturnsNoContent()
    {
        _service
            .Setup(s => s.DeleteAsync(CallerId, VehicleId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _sut.Delete(VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task Delete_WhenNothingDeleted_ReturnsNotFound()
    {
        _service
            .Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var response = await _sut.Delete(VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }
}
