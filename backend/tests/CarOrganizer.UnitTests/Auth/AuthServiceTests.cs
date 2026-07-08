using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CarOrganizer.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManager = CreateUserManagerMock();
    private readonly Mock<IJwtTokenGenerator> _tokenGenerator = new();

    private AuthService CreateSut() => new(_userManager.Object, _tokenGenerator.Object);

    // UserManager<T> has a large constructor; only the store is needed for its methods to be mockable.
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    // ---- Register ----------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_WhenCreationSucceeds_ReturnsSuccess()
    {
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await CreateSut().RegisterAsync(new RegisterRequest("user@example.com", "Passw0rd123"));

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task RegisterAsync_UsesEmailForBothUserNameAndEmail()
    {
        User? captured = null;
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((user, _) => captured = user)
            .ReturnsAsync(IdentityResult.Success);

        await CreateSut().RegisterAsync(new RegisterRequest("user@example.com", "Passw0rd123"));

        Assert.NotNull(captured);
        Assert.Equal("user@example.com", captured!.Email);
        Assert.Equal("user@example.com", captured.UserName);
    }

    [Fact]
    public async Task RegisterAsync_ForwardsThePasswordToUserManager()
    {
        string? capturedPassword = null;
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((_, password) => capturedPassword = password)
            .ReturnsAsync(IdentityResult.Success);

        await CreateSut().RegisterAsync(new RegisterRequest("user@example.com", "Passw0rd123"));

        Assert.Equal("Passw0rd123", capturedPassword);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreationFails_ReturnsFailure()
    {
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "nope" }));

        var result = await CreateSut().RegisterAsync(new RegisterRequest("user@example.com", "weak"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreationFails_MapsAllErrorDescriptions()
    {
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "A", Description = "Password too short." },
                new IdentityError { Code = "B", Description = "Email already taken." }));

        var result = await CreateSut().RegisterAsync(new RegisterRequest("user@example.com", "weak"));

        Assert.Equal(["Password too short.", "Email already taken."], result.Errors);
    }

    // ---- Login -------------------------------------------------------------

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTheGeneratedToken()
    {
        var user = new User { Email = "user@example.com" };
        var expiry = DateTime.UtcNow.AddMinutes(15);
        _userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Passw0rd123")).ReturnsAsync(true);
        _tokenGenerator.Setup(g => g.GenerateAccessToken(user)).Returns(new AccessToken("signed.jwt.token", expiry));

        var result = await CreateSut().LoginAsync(new LoginRequest("user@example.com", "Passw0rd123"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("signed.jwt.token", result.Value!.AccessToken);
        Assert.Equal(expiry, result.Value.AccessTokenExpiresAtUtc);
        Assert.Equal("Bearer", result.Value.TokenType);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ReturnsFailureAndDoesNotGenerateToken()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var result = await CreateSut().LoginAsync(new LoginRequest("ghost@example.com", "Passw0rd123"));

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        _tokenGenerator.Verify(g => g.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailureAndDoesNotGenerateToken()
    {
        var user = new User { Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

        var result = await CreateSut().LoginAsync(new LoginRequest("user@example.com", "wrong-password"));

        Assert.False(result.Succeeded);
        _tokenGenerator.Verify(g => g.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmailAndWrongPassword_ProduceTheSameError()
    {
        // Unknown email
        _userManager.Setup(m => m.FindByEmailAsync("ghost@example.com")).ReturnsAsync((User?)null);
        var unknownEmail = await CreateSut().LoginAsync(new LoginRequest("ghost@example.com", "Passw0rd123"));

        // Known email, wrong password
        var user = new User { Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);
        var wrongPassword = await CreateSut().LoginAsync(new LoginRequest("user@example.com", "wrong-password"));

        Assert.Equal(unknownEmail.Errors, wrongPassword.Errors);
    }
}
