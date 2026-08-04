using System.Security.Claims;
using CarOrganizer.API.Controllers;
using CarOrganizer.Application.Dashboard;
using CarOrganizer.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarOrganizer.UnitTests.Dashboard;

/// <summary>
/// Covers <see cref="DashboardController"/>: passing the owner from the token and the two query
/// parameters through, and handing the service's result back as 200. Range validation is the
/// framework's job and is covered end to end in the integration suite.
/// </summary>
public class DashboardControllerTests
{
    private static readonly Guid CallerId = Guid.NewGuid();

    private readonly Mock<IDashboardService> _service = new();
    private readonly DashboardController _sut;

    public DashboardControllerTests()
    {
        _sut = new DashboardController(_service.Object)
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

    private static DashboardResponse SampleResponse() =>
        new(DateTime.UtcNow, 30, 0, []);

    [Fact]
    public async Task Get_ReturnsOkWithTheServicesResult()
    {
        var dashboard = SampleResponse();
        _service
            .Setup(s => s.GetAsync(CallerId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboard);

        var response = await _sut.Get(cancellationToken: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(dashboard, ok.Value);
    }

    [Fact]
    public async Task Get_UsesTheOwnerFromTheTokenAndTheSuppliedParameters()
    {
        _service
            .Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        await _sut.Get(withinDays: 90, recentCount: 3, cancellationToken: CancellationToken.None);

        _service.Verify(s => s.GetAsync(CallerId, 90, 3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithoutParameters_FallsBackToTheDocumentedDefaults()
    {
        _service
            .Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        await _sut.Get(cancellationToken: CancellationToken.None);

        _service.Verify(
            s => s.GetAsync(
                CallerId,
                DashboardLimits.DefaultWithinDays,
                DashboardLimits.DefaultRecentCount,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
