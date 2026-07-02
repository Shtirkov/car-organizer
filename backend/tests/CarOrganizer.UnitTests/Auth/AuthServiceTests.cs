using CarOrganizer.Application.Auth;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CarOrganizer.UnitTests.Auth;

public class AuthServiceTests
{
    // UserManager<T> has a large constructor; only the store is needed for CreateAsync to be mockable.
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreationSucceeds_ReturnsSuccess()
    {
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        var sut = new AuthService(userManager.Object);

        var result = await sut.RegisterAsync(new RegisterRequest("user@example.com", "Passw0rd123"));

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task RegisterAsync_UsesEmailForBothUserNameAndEmail()
    {
        var userManager = CreateUserManagerMock();
        User? captured = null;
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((user, _) => captured = user)
            .ReturnsAsync(IdentityResult.Success);
        var sut = new AuthService(userManager.Object);

        await sut.RegisterAsync(new RegisterRequest("user@example.com", "Passw0rd123"));

        Assert.NotNull(captured);
        Assert.Equal("user@example.com", captured!.Email);
        Assert.Equal("user@example.com", captured.UserName);
    }

    [Fact]
    public async Task RegisterAsync_ForwardsThePasswordToUserManager()
    {
        var userManager = CreateUserManagerMock();
        string? capturedPassword = null;
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((_, password) => capturedPassword = password)
            .ReturnsAsync(IdentityResult.Success);
        var sut = new AuthService(userManager.Object);

        await sut.RegisterAsync(new RegisterRequest("user@example.com", "Passw0rd123"));

        Assert.Equal("Passw0rd123", capturedPassword);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreationFails_ReturnsFailure()
    {
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "nope" }));
        var sut = new AuthService(userManager.Object);

        var result = await sut.RegisterAsync(new RegisterRequest("user@example.com", "weak"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreationFails_MapsAllErrorDescriptions()
    {
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "A", Description = "Password too short." },
                new IdentityError { Code = "B", Description = "Email already taken." }));
        var sut = new AuthService(userManager.Object);

        var result = await sut.RegisterAsync(new RegisterRequest("user@example.com", "weak"));

        Assert.Equal(["Password too short.", "Email already taken."], result.Errors);
    }
}
