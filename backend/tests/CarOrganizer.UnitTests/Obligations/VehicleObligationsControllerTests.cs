using System.Security.Claims;
using CarOrganizer.API.Controllers;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.Obligations;
using CarOrganizer.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarOrganizer.UnitTests.Obligations;

/// <summary>
/// Covers <see cref="VehicleObligationsController"/>: mapping the service's outcome to the right HTTP
/// result, and passing the owner (from the token) and vehicle (from the route) into the service.
/// </summary>
public class VehicleObligationsControllerTests
{
    private static readonly Guid CallerId = Guid.NewGuid();
    private static readonly Guid VehicleId = Guid.NewGuid();

    private readonly Mock<IVehicleObligationService> _service = new();
    private readonly VehicleObligationsController _sut;

    public VehicleObligationsControllerTests()
    {
        _sut = new VehicleObligationsController(_service.Object)
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

    private static VehicleObligationResponse SampleResponse(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), ObligationType.Insurance, null, new DateOnly(2026, 12, 31), 450m, null, null, null, DateTime.UtcNow, null);

    private static CreateVehicleObligationRequest CreateRequest() =>
        new(ObligationType.Insurance, null, new DateOnly(2026, 12, 31), 450m, null, null, null);

    private static UpdateVehicleObligationRequest UpdateRequest() =>
        new(ObligationType.Casco, null, new DateOnly(2027, 2, 28), 1200m, null, null, null);

    [Fact]
    public async Task List_WhenServiceReturnsObligations_ReturnsOk()
    {
        _service
            .Setup(s => s.ListAsync(CallerId, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([SampleResponse()]);

        var response = await _sut.List(VehicleId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        var obligations = Assert.IsAssignableFrom<IReadOnlyList<VehicleObligationResponse>>(ok.Value);
        Assert.Single(obligations);
    }

    [Fact]
    public async Task List_WhenVehicleNotFound_ReturnsNotFound()
    {
        _service
            .Setup(s => s.ListAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<VehicleObligationResponse>?)null);

        var response = await _sut.List(VehicleId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Get_WhenFound_ReturnsOk()
    {
        var obligation = SampleResponse();
        _service
            .Setup(s => s.GetAsync(CallerId, VehicleId, obligation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(obligation);

        var response = await _sut.Get(VehicleId, obligation.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(obligation, ok.Value);
    }

    [Fact]
    public async Task Get_WhenMissing_ReturnsNotFound()
    {
        _service
            .Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleObligationResponse?)null);

        var response = await _sut.Get(VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Create_WhenCreated_ReturnsCreatedAtTheObligationsUrlWithBothRouteValues()
    {
        var obligation = SampleResponse();
        _service
            .Setup(s => s.CreateAsync(CallerId, VehicleId, It.IsAny<CreateVehicleObligationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(obligation);

        var response = await _sut.Create(VehicleId, CreateRequest(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(nameof(VehicleObligationsController.Get), created.ActionName);
        Assert.Equal(VehicleId, created.RouteValues!["vehicleId"]);
        Assert.Equal(obligation.Id, created.RouteValues["id"]);
        Assert.Same(obligation, created.Value);
    }

    [Fact]
    public async Task Create_UsesTheOwnerFromTheTokenAndVehicleFromTheRoute()
    {
        var request = CreateRequest();
        _service
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateVehicleObligationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        await _sut.Create(VehicleId, request, CancellationToken.None);

        _service.Verify(s => s.CreateAsync(CallerId, VehicleId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenVehicleNotFound_ReturnsNotFound()
    {
        _service
            .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateVehicleObligationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleObligationResponse?)null);

        var response = await _sut.Create(VehicleId, CreateRequest(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Update_WhenUpdated_ReturnsOk()
    {
        var obligation = SampleResponse();
        _service
            .Setup(s => s.UpdateAsync(CallerId, VehicleId, obligation.Id, It.IsAny<UpdateVehicleObligationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(obligation);

        var response = await _sut.Update(VehicleId, obligation.Id, UpdateRequest(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(obligation, ok.Value);
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        _service
            .Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateVehicleObligationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleObligationResponse?)null);

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
