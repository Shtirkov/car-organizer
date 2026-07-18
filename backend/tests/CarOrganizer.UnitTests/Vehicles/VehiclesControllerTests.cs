using System.Security.Claims;
using CarOrganizer.API.Controllers;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.Vehicles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarOrganizer.UnitTests.Vehicles;

/// <summary>
/// Covers <see cref="VehiclesController"/>'s job: turning what the service reports into the right
/// HTTP result, and passing the owner from the token (never the body) into the service.
/// </summary>
public class VehiclesControllerTests
{
    private static readonly Guid CallerId = Guid.NewGuid();

    private readonly Mock<IVehicleService> _service = new();
    private readonly VehiclesController _sut;

    public VehiclesControllerTests()
    {
        _sut = new VehiclesController(_service.Object)
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

    private static VehicleResponse SampleResponse(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), "Audi", "A4", 2015, 150_000, 190_000, null, null, null, DateTime.UtcNow, null);

    private static CreateVehicleRequest SampleCreateRequest() =>
        new("Audi", "A4", 2015, 150_000, 190_000, null, null, null);

    private static UpdateVehicleRequest SampleUpdateRequest() =>
        new("Audi", "A6", 2016, 150_000, 200_000, null, null, null);

    [Fact]
    public async Task List_ReturnsOkWithTheGarage()
    {
        _service
            .Setup(s => s.ListAsync(CallerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([SampleResponse(), SampleResponse()]);

        var response = await _sut.List(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        var vehicles = Assert.IsAssignableFrom<IReadOnlyList<VehicleResponse>>(ok.Value);
        Assert.Equal(2, vehicles.Count);
    }

    [Fact]
    public async Task List_AsksTheServiceForTheCallersOwnGarage()
    {
        _service
            .Setup(s => s.ListAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.List(CancellationToken.None);

        _service.Verify(s => s.ListAsync(CallerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WhenTheVehicleExists_ReturnsOkWithIt()
    {
        var vehicle = SampleResponse();
        _service
            .Setup(s => s.GetAsync(CallerId, vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var response = await _sut.Get(vehicle.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(vehicle, ok.Value);
    }

    [Fact]
    public async Task Get_WhenTheServiceReportsNothing_ReturnsNotFound()
    {
        _service
            .Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleResponse?)null);

        var response = await _sut.Get(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtTheVehiclesOwnUrl()
    {
        var vehicle = SampleResponse();
        _service
            .Setup(s => s.CreateAsync(CallerId, It.IsAny<CreateVehicleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var response = await _sut.Create(SampleCreateRequest(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(nameof(VehiclesController.Get), created.ActionName);
        Assert.Equal(vehicle.Id, created.RouteValues!["id"]);
        Assert.Same(vehicle, created.Value);
    }

    [Fact]
    public async Task Create_AssignsTheVehicleToTheCallerFromTheToken()
    {
        _service
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateVehicleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        await _sut.Create(SampleCreateRequest(), CancellationToken.None);

        _service.Verify(
            s => s.CreateAsync(CallerId, It.IsAny<CreateVehicleRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_PassesTheRequestToTheService()
    {
        var request = SampleCreateRequest();
        _service
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateVehicleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        await _sut.Create(request, CancellationToken.None);

        _service.Verify(s => s.CreateAsync(CallerId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenTheVehicleExists_ReturnsOkWithTheUpdatedVehicle()
    {
        var vehicle = SampleResponse();
        _service
            .Setup(s => s.UpdateAsync(CallerId, vehicle.Id, It.IsAny<UpdateVehicleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var response = await _sut.Update(vehicle.Id, SampleUpdateRequest(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(vehicle, ok.Value);
    }

    [Fact]
    public async Task Update_WhenTheServiceReportsNothing_ReturnsNotFound()
    {
        _service
            .Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateVehicleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleResponse?)null);

        var response = await _sut.Update(Guid.NewGuid(), SampleUpdateRequest(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Delete_WhenTheVehicleWasDeleted_ReturnsNoContent()
    {
        _service
            .Setup(s => s.DeleteAsync(CallerId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task Delete_WhenTheServiceReportsNothingDeleted_ReturnsNotFound()
    {
        _service
            .Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var response = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }
}
