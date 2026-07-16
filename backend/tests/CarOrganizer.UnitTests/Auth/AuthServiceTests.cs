using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace CarOrganizer.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManager = CreateUserManagerMock();
    private readonly Mock<IJwtTokenGenerator> _tokenGenerator = new();
    private readonly Mock<IRefreshTokenStore> _refreshTokenStore = new();
    private readonly JwtSettings _jwtSettings = new() { RefreshTokenDays = 7 };

    private AuthService CreateSut() => new(
        _userManager.Object,
        _tokenGenerator.Object,
        _refreshTokenStore.Object,
        Options.Create(_jwtSettings));

    // UserManager<T> has a large constructor; only the store is needed for its methods to be mockable.
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static AccessToken AnyAccessToken() => new("signed.jwt.token", DateTime.UtcNow.AddMinutes(15));

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
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
        Assert.True(result.Value.RefreshTokenExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_PersistsARefreshTokenStoredAsAHashNotTheRawValue()
    {
        var user = new User { Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Passw0rd123")).ReturnsAsync(true);
        _tokenGenerator.Setup(g => g.GenerateAccessToken(user)).Returns(AnyAccessToken());
        RefreshToken? stored = null;
        _refreshTokenStore
            .Setup(s => s.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => stored = token)
            .Returns(Task.CompletedTask);

        var result = await CreateSut().LoginAsync(new LoginRequest("user@example.com", "Passw0rd123"));

        Assert.NotNull(stored);
        Assert.Equal(user.Id, stored!.UserId);
        // The stored hash must NOT equal the raw token handed to the client.
        Assert.NotEqual(result.Value!.RefreshToken, stored.TokenHash);
        Assert.False(string.IsNullOrWhiteSpace(stored.TokenHash));
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

    // ---- Refresh -----------------------------------------------------------

    // Builds a stored token whose hash matches the SHA-256 of the given raw token, mirroring AuthService.
    private static RefreshToken ActiveTokenFor(string rawToken, User user) => new()
    {
        User = user,
        UserId = user.Id,
        TokenHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken))),
        ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
    };

    [Fact]
    public async Task RefreshAsync_WithActiveToken_ReturnsNewTokenPair()
    {
        var user = new User { Email = "user@example.com" };
        var stored = ActiveTokenFor("raw-refresh-token", user);
        _refreshTokenStore.Setup(s => s.FindByHashAsync(stored.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        _tokenGenerator.Setup(g => g.GenerateAccessToken(user)).Returns(AnyAccessToken());

        var result = await CreateSut().RefreshAsync(new RefreshRequest("raw-refresh-token"));

        Assert.True(result.Succeeded);
        Assert.Equal("signed.jwt.token", result.Value!.AccessToken);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
    }

    [Fact]
    public async Task RefreshAsync_RotatesTheToken_RevokingTheOldAndPersistingANewOne()
    {
        var user = new User { Email = "user@example.com" };
        var stored = ActiveTokenFor("raw-refresh-token", user);
        _refreshTokenStore.Setup(s => s.FindByHashAsync(stored.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        _tokenGenerator.Setup(g => g.GenerateAccessToken(user)).Returns(AnyAccessToken());

        await CreateSut().RefreshAsync(new RefreshRequest("raw-refresh-token"));

        Assert.NotNull(stored.RevokedAtUtc); // old token revoked
        _refreshTokenStore.Verify(s => s.UpdateAsync(stored, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenStore.Verify(s => s.AddAsync(It.Is<RefreshToken>(t => t.RevokedAtUtc == null), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WithUnknownToken_ReturnsFailureAndIssuesNothing()
    {
        _refreshTokenStore.Setup(s => s.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        var result = await CreateSut().RefreshAsync(new RefreshRequest("does-not-exist"));

        Assert.False(result.Succeeded);
        _tokenGenerator.Verify(g => g.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _refreshTokenStore.Verify(s => s.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredToken_ReturnsFailure()
    {
        var user = new User { Email = "user@example.com" };
        var expired = ActiveTokenFor("raw-refresh-token", user);
        expired.ExpiresAtUtc = DateTime.UtcNow.AddDays(-1);
        _refreshTokenStore.Setup(s => s.FindByHashAsync(expired.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(expired);

        var result = await CreateSut().RefreshAsync(new RefreshRequest("raw-refresh-token"));

        Assert.False(result.Succeeded);
        _tokenGenerator.Verify(g => g.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_WithAlreadyRevokedToken_ReturnsFailure()
    {
        var user = new User { Email = "user@example.com" };
        var revoked = ActiveTokenFor("raw-refresh-token", user);
        revoked.RevokedAtUtc = DateTime.UtcNow.AddMinutes(-5);
        _refreshTokenStore.Setup(s => s.FindByHashAsync(revoked.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(revoked);

        var result = await CreateSut().RefreshAsync(new RefreshRequest("raw-refresh-token"));

        Assert.False(result.Succeeded);
        _tokenGenerator.Verify(g => g.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    // ---- Logout ------------------------------------------------------------

    [Fact]
    public async Task LogoutAsync_WithActiveToken_RevokesIt()
    {
        var stored = ActiveTokenFor("raw-refresh-token", new User { Email = "user@example.com" });
        _refreshTokenStore.Setup(s => s.FindByHashAsync(stored.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(stored);

        await CreateSut().LogoutAsync(new LogoutRequest("raw-refresh-token"));

        Assert.NotNull(stored.RevokedAtUtc);
        _refreshTokenStore.Verify(s => s.UpdateAsync(stored, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WithUnknownToken_DoesNothing()
    {
        _refreshTokenStore.Setup(s => s.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        await CreateSut().LogoutAsync(new LogoutRequest("does-not-exist"));

        _refreshTokenStore.Verify(s => s.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithAlreadyRevokedToken_IsANoOp()
    {
        var revoked = ActiveTokenFor("raw-refresh-token", new User { Email = "user@example.com" });
        revoked.RevokedAtUtc = DateTime.UtcNow.AddMinutes(-5);
        _refreshTokenStore.Setup(s => s.FindByHashAsync(revoked.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(revoked);

        await CreateSut().LogoutAsync(new LogoutRequest("raw-refresh-token"));

        _refreshTokenStore.Verify(s => s.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
