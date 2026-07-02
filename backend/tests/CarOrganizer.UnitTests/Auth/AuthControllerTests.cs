using CarOrganizer.API.Controllers;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Common;
using CarOrganizer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarOrganizer.UnitTests.Auth;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_WhenServiceSucceeds_ReturnsOk()
    {
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var sut = new AuthController(service.Object);

        var response = await sut.Register(new RegisterRequest("user@example.com", "Passw0rd123"), CancellationToken.None);

        Assert.IsType<OkResult>(response);
    }

    [Fact]
    public async Task Register_WhenServiceFails_ReturnsBadRequest()
    {
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(["bad"]));
        var sut = new AuthController(service.Object);

        var response = await sut.Register(new RegisterRequest("user@example.com", "weak"), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response);
    }

    [Fact]
    public async Task Register_WhenServiceFails_BadRequestCarriesTheErrors()
    {
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(["first error", "second error"]));
        var sut = new AuthController(service.Object);

        var response = await sut.Register(new RegisterRequest("user@example.com", "weak"), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(response);
        var errorsProperty = badRequest.Value!.GetType().GetProperty("errors");
        Assert.NotNull(errorsProperty);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(errorsProperty!.GetValue(badRequest.Value));
        Assert.Equal(["first error", "second error"], errors);
    }

    [Fact]
    public async Task Register_PassesTheRequestToTheService()
    {
        var request = new RegisterRequest("user@example.com", "Passw0rd123");
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var sut = new AuthController(service.Object);

        await sut.Register(request, CancellationToken.None);

        service.Verify(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_ForwardsTheCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var sut = new AuthController(service.Object);

        await sut.Register(new RegisterRequest("user@example.com", "Passw0rd123"), cts.Token);

        service.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), cts.Token), Times.Once);
    }
}
