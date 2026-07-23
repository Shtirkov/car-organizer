using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Obligations;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Domain.Enums;
using CarOrganizer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// End-to-end tests for /api/vehicles/{vehicleId}/obligations over the full HTTP pipeline. Clients
/// register and log in for real (so the vehicle's OwnerId is a genuine user, as the FK demands).
/// A fresh factory (and in-memory database) is created per test to keep them isolated.
///
/// The enum <c>Type</c> is sent as its numeric value — the API uses the default System.Text.Json
/// numeric enum format.
/// </summary>
public class VehicleObligationEndpointsTests : IDisposable
{
    private const string Password = "Passw0rd!23";

    private readonly CustomWebApplicationFactory _factory;

    public VehicleObligationEndpointsTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<HttpClient> SignUpAsync(string email)
    {
        var client = _factory.CreateClient();
        var credentials = new { email, password = Password };

        await client.PostAsJsonAsync("/api/auth/register", credentials);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", credentials);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        return client;
    }

    private static async Task<VehicleResponse> CreateVehicleAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/vehicles",
            new { make = "Audi", model = "A4", year = 2015, purchaseMileage = 150_000, currentMileage = 190_000 });
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>())!;
    }

    private static string ObligationsUrl(Guid vehicleId) => $"/api/vehicles/{vehicleId}/obligations";

    private static object SampleObligation(
        ObligationType type = ObligationType.Insurance,
        string validUntil = "2026-12-31") =>
        new
        {
            type = (int)type,
            validFrom = "2026-01-01",
            validUntil,
            cost = 450.00,
            provider = "Bulstrad",
            policyNumber = "BG/03/123456789",
            notes = "Full year",
        };

    private static async Task<VehicleObligationResponse> CreateObligationAsync(
        HttpClient client, Guid vehicleId, ObligationType type = ObligationType.Insurance, string validUntil = "2026-12-31")
    {
        var response = await client.PostAsJsonAsync(ObligationsUrl(vehicleId), SampleObligation(type, validUntil));
        return (await response.Content.ReadFromJsonAsync<VehicleObligationResponse>())!;
    }

    // ---------- authentication ----------

    [Fact]
    public async Task List_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(ObligationsUrl(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(ObligationsUrl(Guid.NewGuid()), SampleObligation());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- create ----------

    [Fact]
    public async Task Create_WithAValidPayload_ReturnsCreated()
    {
        using var client = await SignUpAsync("ob.create@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(ObligationsUrl(vehicle.Id), SampleObligation());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_PointsLocationAtTheNewObligation()
    {
        using var client = await SignUpAsync("ob.location@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(ObligationsUrl(vehicle.Id), SampleObligation());

        Assert.NotNull(response.Headers.Location);
        var followUp = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, followUp.StatusCode);
    }

    [Fact]
    public async Task Create_PersistsTheObligationAgainstTheVehicle()
    {
        using var client = await SignUpAsync("ob.persist@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var created = await CreateObligationAsync(client, vehicle.Id);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var obligation = await db.Obligations.SingleAsync();
        Assert.Equal(created.Id, obligation.Id);
        Assert.Equal(vehicle.Id, obligation.VehicleId);
        Assert.Equal(ObligationType.Insurance, obligation.Type);
        Assert.Equal(new DateOnly(2026, 12, 31), obligation.ValidUntil);
        Assert.Equal(450.00m, obligation.Cost);
    }

    [Fact]
    public async Task Create_RoundTripsTheValidityDates()
    {
        using var client = await SignUpAsync("ob.dates@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var created = await CreateObligationAsync(client, vehicle.Id, validUntil: "2027-03-15");

        Assert.Equal(new DateOnly(2026, 1, 1), created.ValidFrom);
        Assert.Equal(new DateOnly(2027, 3, 15), created.ValidUntil);
    }

    [Fact]
    public async Task Create_WithoutOptionalFields_Succeeds()
    {
        using var client = await SignUpAsync("ob.minimal@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(
            ObligationsUrl(vehicle.Id), new { type = (int)ObligationType.Tax, validUntil = "2026-12-31", cost = 87 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var obligation = await response.Content.ReadFromJsonAsync<VehicleObligationResponse>();
        Assert.Null(obligation!.ValidFrom);
        Assert.Null(obligation.Provider);
        Assert.Null(obligation.PolicyNumber);
    }

    [Fact]
    public async Task Create_DoesNotLeakTheVehicleId()
    {
        using var client = await SignUpAsync("ob.noleak@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(ObligationsUrl(vehicle.Id), SampleObligation());
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("vehicleId", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WithValidFromAfterValidUntil_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("ob.baddates@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(
            ObligationsUrl(vehicle.Id),
            new { type = 1, validFrom = "2026-12-31", validUntil = "2026-01-01", cost = 100 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAnUndefinedType_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("ob.badtype@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(
            ObligationsUrl(vehicle.Id), new { type = 999, validUntil = "2026-12-31", cost = 100 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ForAnotherUsersVehicle_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("ob.alice.create@example.com");
        using var bob = await SignUpAsync("ob.bob.create@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);

        var response = await bob.PostAsJsonAsync(ObligationsUrl(aliceVehicle.Id), SampleObligation());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ForAnUnknownVehicle_ReturnsNotFound()
    {
        using var client = await SignUpAsync("ob.novehicle@example.com");

        var response = await client.PostAsJsonAsync(ObligationsUrl(Guid.NewGuid()), SampleObligation());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- read ----------

    [Fact]
    public async Task Get_ReturnsTheObligation()
    {
        using var client = await SignUpAsync("ob.get@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var created = await CreateObligationAsync(client, vehicle.Id);

        var response = await client.GetAsync($"{ObligationsUrl(vehicle.Id)}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var obligation = await response.Content.ReadFromJsonAsync<VehicleObligationResponse>();
        Assert.Equal(created.Id, obligation!.Id);
    }

    [Fact]
    public async Task List_WithNoObligations_ReturnsEmpty()
    {
        using var client = await SignUpAsync("ob.empty@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var obligations = await client.GetFromJsonAsync<List<VehicleObligationResponse>>(ObligationsUrl(vehicle.Id));

        Assert.Empty(obligations!);
    }

    [Fact]
    public async Task List_ForAnUnknownVehicle_ReturnsNotFound()
    {
        using var client = await SignUpAsync("ob.listunknown@example.com");

        var response = await client.GetAsync(ObligationsUrl(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsObligationsLatestExpiryFirst()
    {
        using var client = await SignUpAsync("ob.order@example.com");
        var vehicle = await CreateVehicleAsync(client);

        await CreateObligationAsync(client, vehicle.Id, ObligationType.Vignette, validUntil: "2026-06-30");
        await CreateObligationAsync(client, vehicle.Id, ObligationType.Insurance, validUntil: "2027-03-15");
        await CreateObligationAsync(client, vehicle.Id, ObligationType.Tax, validUntil: "2026-12-31");

        var obligations = await client.GetFromJsonAsync<List<VehicleObligationResponse>>(ObligationsUrl(vehicle.Id));

        Assert.Equal(
            [new DateOnly(2027, 3, 15), new DateOnly(2026, 12, 31), new DateOnly(2026, 6, 30)],
            obligations!.Select(o => o.ValidUntil));
    }

    // ---------- update / delete ----------

    [Fact]
    public async Task Update_ReturnsTheUpdatedObligationAndPersistsIt()
    {
        using var client = await SignUpAsync("ob.update@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var obligation = await CreateObligationAsync(client, vehicle.Id);

        var response = await client.PutAsJsonAsync(
            $"{ObligationsUrl(vehicle.Id)}/{obligation.Id}",
            new { type = (int)ObligationType.Casco, validFrom = "2026-03-01", validUntil = "2027-02-28", cost = 1200, provider = "DZI", policyNumber = "CASCO-987" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<VehicleObligationResponse>();
        Assert.Equal(ObligationType.Casco, updated!.Type);
        Assert.Equal(new DateOnly(2027, 2, 28), updated.ValidUntil);
        Assert.Equal("DZI", updated.Provider);
    }

    [Fact]
    public async Task Update_ForAnUnknownObligation_ReturnsNotFound()
    {
        using var client = await SignUpAsync("ob.updateunknown@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PutAsJsonAsync(
            $"{ObligationsUrl(vehicle.Id)}/{Guid.NewGuid()}",
            new { type = 1, validUntil = "2026-12-31", cost = 100 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContentAndTheObligationIsGone()
    {
        using var client = await SignUpAsync("ob.delete@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var obligation = await CreateObligationAsync(client, vehicle.Id);

        var response = await client.DeleteAsync($"{ObligationsUrl(vehicle.Id)}/{obligation.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var followUp = await client.GetAsync($"{ObligationsUrl(vehicle.Id)}/{obligation.Id}");
        Assert.Equal(HttpStatusCode.NotFound, followUp.StatusCode);
    }

    // ---------- isolation between users ----------

    [Fact]
    public async Task Get_AnotherUsersObligation_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("ob.alice.get@example.com");
        using var bob = await SignUpAsync("ob.bob.get@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        var aliceObligation = await CreateObligationAsync(alice, aliceVehicle.Id);

        var response = await bob.GetAsync($"{ObligationsUrl(aliceVehicle.Id)}/{aliceObligation.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AnotherUsersObligation_ReturnsNotFoundAndLeavesItAlone()
    {
        using var alice = await SignUpAsync("ob.alice.delete@example.com");
        using var bob = await SignUpAsync("ob.bob.delete@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        var aliceObligation = await CreateObligationAsync(alice, aliceVehicle.Id);

        var response = await bob.DeleteAsync($"{ObligationsUrl(aliceVehicle.Id)}/{aliceObligation.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await db.Obligations.AnyAsync(o => o.Id == aliceObligation.Id));
    }
}
