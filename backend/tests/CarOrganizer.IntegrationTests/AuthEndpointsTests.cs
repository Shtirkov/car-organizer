using System.Net;
using System.Net.Http.Json;
using CarOrganizer.Application.Auth;
using CarOrganizer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// End-to-end tests for POST /api/auth/register that exercise the full HTTP pipeline:
/// model validation, the controller, the Identity-backed service, and persistence.
/// A fresh factory (and in-memory database) is created per test to keep them isolated.
/// </summary>
public class AuthEndpointsTests : IDisposable
{
    private const string RegisterUrl = "/api/auth/register";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Register_WithValidCredentials_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync(
            RegisterUrl, new { email = "valid@example.com", password = "Passw0rd123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidCredentials_PersistsUserWithHashedPassword()
    {
        await _client.PostAsJsonAsync(
            RegisterUrl, new { email = "stored@example.com", password = "Passw0rd123" });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == "stored@example.com");

        Assert.NotNull(user);
        Assert.False(string.IsNullOrWhiteSpace(user!.PasswordHash));
        Assert.NotEqual("Passw0rd123", user.PasswordHash);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var payload = new { email = "dupe@example.com", password = "Passw0rd123" };
        await _client.PostAsJsonAsync(RegisterUrl, payload);

        var second = await _client.PostAsJsonAsync(RegisterUrl, payload);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            RegisterUrl, new { email = "weak@example.com", password = "ab1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmailFormat_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            RegisterUrl, new { email = "not-an-email", password = "Passw0rd123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            RegisterUrl, new { email = "nopwd@example.com" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingEmail_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            RegisterUrl, new { password = "Passw0rd123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- Login -------------------------------------------------------------

    private const string LoginUrl = "/api/auth/login";

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsOkWithAToken()
    {
        var credentials = new { email = "login@example.com", password = "Passw0rd123" };
        await _client.PostAsJsonAsync(RegisterUrl, credentials);

        var response = await _client.PostAsJsonAsync(LoginUrl, credentials);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.Equal(3, body!.AccessToken.Split('.').Length);
        Assert.Equal("Bearer", body.TokenType);
        Assert.True(body.AccessTokenExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await _client.PostAsJsonAsync(RegisterUrl, new { email = "wrongpw@example.com", password = "Passw0rd123" });

        var response = await _client.PostAsJsonAsync(
            LoginUrl, new { email = "wrongpw@example.com", password = "NotThePassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            LoginUrl, new { email = "ghost@example.com", password = "Passw0rd123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            LoginUrl, new { email = "not-an-email", password = "Passw0rd123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(LoginUrl, new { email = "nopwd@example.com" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
