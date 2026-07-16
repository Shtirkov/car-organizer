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

    // ---- Login -------------------------------------------------------------

    [Fact]
    public async Task Login_WhenServiceSucceeds_ReturnsOkWithAuthResponse()
    {
        var authResponse = new AuthResponse("signed.jwt.token", DateTime.UtcNow.AddMinutes(15), "refresh-token", DateTime.UtcNow.AddDays(7));
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponse>.Success(authResponse));
        var sut = new AuthController(service.Object);

        var response = await sut.Login(new LoginRequest("user@example.com", "Passw0rd123"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(authResponse, ok.Value);
    }

    [Fact]
    public async Task Login_WhenServiceFails_ReturnsUnauthorized()
    {
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponse>.Failure(["Invalid email or password."]));
        var sut = new AuthController(service.Object);

        var response = await sut.Login(new LoginRequest("user@example.com", "wrong"), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(response);
    }

    [Fact]
    public async Task Login_PassesTheRequestAndTokenToTheService()
    {
        using var cts = new CancellationTokenSource();
        var request = new LoginRequest("user@example.com", "Passw0rd123");
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponse>.Success(new AuthResponse("t", DateTime.UtcNow, "r", DateTime.UtcNow.AddDays(7))));
        var sut = new AuthController(service.Object);

        await sut.Login(request, cts.Token);

        service.Verify(s => s.LoginAsync(request, cts.Token), Times.Once);
    }

    // ---- Refresh -----------------------------------------------------------

    [Fact]
    public async Task Refresh_WhenServiceSucceeds_ReturnsOkWithAuthResponse()
    {
        var authResponse = new AuthResponse("new.access.token", DateTime.UtcNow.AddMinutes(15), "new-refresh", DateTime.UtcNow.AddDays(7));
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponse>.Success(authResponse));
        var sut = new AuthController(service.Object);

        var response = await sut.Refresh(new RefreshRequest("old-refresh"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(authResponse, ok.Value);
    }

    [Fact]
    public async Task Refresh_WhenServiceFails_ReturnsUnauthorized()
    {
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponse>.Failure(["Invalid or expired refresh token."]));
        var sut = new AuthController(service.Object);

        var response = await sut.Refresh(new RefreshRequest("bad"), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(response);
    }
}
