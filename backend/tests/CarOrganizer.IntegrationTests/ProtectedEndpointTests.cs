using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarOrganizer.Application.Auth;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// Exercises the JWT validation middleware via the protected GET /api/auth/me endpoint.
/// A fresh factory (and in-memory database) is created per test to keep them isolated.
/// </summary>
public class ProtectedEndpointTests : IDisposable
{
    private const string MeUrl = "/api/auth/me";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProtectedEndpointTests()
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

    private void UseBearer(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync(MeUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsTheIdentityFromTheToken()
    {
        var userId = Guid.NewGuid().ToString();
        UseBearer(TestJwt.Create(sub: userId, email: "me@example.com"));

        var response = await _client.GetAsync(MeUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.NotNull(body);
        Assert.Equal(userId, body!.Id);
        Assert.Equal("me@example.com", body.Email);
    }

    [Fact]
    public async Task Me_WithExpiredToken_ReturnsUnauthorized()
    {
        UseBearer(TestJwt.Create(sub: Guid.NewGuid().ToString(), expires: DateTime.UtcNow.AddMinutes(-1)));

        var response = await _client.GetAsync(MeUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithTokenSignedByWrongKey_ReturnsUnauthorized()
    {
        UseBearer(TestJwt.Create(sub: Guid.NewGuid().ToString(), key: "a-totally-different-signing-key-also-32-bytes-long!"));

        var response = await _client.GetAsync(MeUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithWrongIssuer_ReturnsUnauthorized()
    {
        UseBearer(TestJwt.Create(sub: Guid.NewGuid().ToString(), issuer: "some-other-issuer"));

        var response = await _client.GetAsync(MeUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithTokenFromRealLogin_ReturnsOk()
    {
        var credentials = new { email = "realflow@example.com", password = "Passw0rd!23" };
        await _client.PostAsJsonAsync("/api/auth/register", credentials);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", credentials);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        UseBearer(auth!.AccessToken);

        var response = await _client.GetAsync(MeUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.Equal("realflow@example.com", body!.Email);
    }

    private sealed record MeResponse(string Id, string Email);
}
