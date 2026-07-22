using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.MaintenanceRecords;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Domain.Enums;
using CarOrganizer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// End-to-end tests for /api/vehicles/{vehicleId}/maintenance-records over the full HTTP pipeline.
/// Clients register and log in for real (so the vehicle's OwnerId is a genuine user, as the FK
/// demands). A fresh factory (and in-memory database) is created per test to keep them isolated.
///
/// The enum <c>Type</c> is sent as its numeric value — the API uses the default System.Text.Json
/// numeric enum format.
/// </summary>
public class MaintenanceRecordEndpointsTests : IDisposable
{
    private const string Password = "Passw0rd!23";

    private readonly CustomWebApplicationFactory _factory;

    public MaintenanceRecordEndpointsTests()
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

    /// <summary>Creates a vehicle and returns it. Purchase 150k, current 190k unless overridden.</summary>
    private static async Task<VehicleResponse> CreateVehicleAsync(HttpClient client, int currentMileage = 190_000)
    {
        var response = await client.PostAsJsonAsync(
            "/api/vehicles",
            new { make = "Audi", model = "A4", year = 2015, purchaseMileage = 150_000, currentMileage });
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>())!;
    }

    private static string RecordsUrl(Guid vehicleId) => $"/api/vehicles/{vehicleId}/maintenance-records";

    private static object SampleRecord(int mileage = 195_000, MaintenanceType type = MaintenanceType.OilChange) =>
        new { type = (int)type, date = "2026-06-01", mileage, cost = 120.50, notes = "Synthetic 5W-30" };

    private static async Task<MaintenanceRecordResponse> CreateRecordAsync(HttpClient client, Guid vehicleId, int mileage = 195_000)
    {
        var response = await client.PostAsJsonAsync(RecordsUrl(vehicleId), SampleRecord(mileage));
        return (await response.Content.ReadFromJsonAsync<MaintenanceRecordResponse>())!;
    }

    private async Task<int> CurrentMileageOfAsync(Guid vehicleId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var vehicle = await db.Vehicles.SingleAsync(v => v.Id == vehicleId);
        return vehicle.CurrentMileage;
    }

    // ---------- authentication ----------

    [Fact]
    public async Task List_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(RecordsUrl(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(RecordsUrl(Guid.NewGuid()), SampleRecord());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- create ----------

    [Fact]
    public async Task Create_WithAValidPayload_ReturnsCreated()
    {
        using var client = await SignUpAsync("mr.create@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(RecordsUrl(vehicle.Id), SampleRecord());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_PointsLocationAtTheNewRecord()
    {
        using var client = await SignUpAsync("mr.location@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(RecordsUrl(vehicle.Id), SampleRecord());

        Assert.NotNull(response.Headers.Location);
        var followUp = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, followUp.StatusCode);
    }

    [Fact]
    public async Task Create_PersistsTheRecordAgainstTheVehicle()
    {
        using var client = await SignUpAsync("mr.persist@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var created = await CreateRecordAsync(client, vehicle.Id);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var record = await db.MaintenanceRecords.SingleAsync();
        Assert.Equal(created.Id, record.Id);
        Assert.Equal(vehicle.Id, record.VehicleId);
        Assert.Equal(MaintenanceType.OilChange, record.Type);
        Assert.Equal(120.50m, record.Cost);
    }

    [Fact]
    public async Task Create_DoesNotLeakTheVehicleId()
    {
        using var client = await SignUpAsync("mr.noleak@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(RecordsUrl(vehicle.Id), SampleRecord());
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("vehicleId", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_ForAnotherUsersVehicle_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("mr.alice.create@example.com");
        using var bob = await SignUpAsync("mr.bob.create@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);

        var response = await bob.PostAsJsonAsync(RecordsUrl(aliceVehicle.Id), SampleRecord());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ForAnUnknownVehicle_ReturnsNotFound()
    {
        using var client = await SignUpAsync("mr.novehicle@example.com");

        var response = await client.PostAsJsonAsync(RecordsUrl(Guid.NewGuid()), SampleRecord());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithNegativeMileage_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("mr.badmileage@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(
            RecordsUrl(vehicle.Id), new { type = 1, date = "2026-06-01", mileage = -1, cost = 10 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAnUndefinedType_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("mr.badtype@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsJsonAsync(
            RecordsUrl(vehicle.Id), new { type = 999, date = "2026-06-01", mileage = 195_000, cost = 10 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- auto-advance ----------

    [Fact]
    public async Task Create_WithMileageAboveCurrent_AdvancesTheVehiclesCurrentMileage()
    {
        using var client = await SignUpAsync("mr.advance@example.com");
        var vehicle = await CreateVehicleAsync(client, currentMileage: 190_000);

        await CreateRecordAsync(client, vehicle.Id, mileage: 200_000);

        Assert.Equal(200_000, await CurrentMileageOfAsync(vehicle.Id));
    }

    [Fact]
    public async Task Create_WithMileageBelowCurrent_LeavesCurrentMileageUnchanged()
    {
        using var client = await SignUpAsync("mr.noadvance@example.com");
        var vehicle = await CreateVehicleAsync(client, currentMileage: 190_000);

        await CreateRecordAsync(client, vehicle.Id, mileage: 185_000);

        Assert.Equal(190_000, await CurrentMileageOfAsync(vehicle.Id));
    }

    [Fact]
    public async Task Update_WithMileageAboveCurrent_AdvancesTheVehiclesCurrentMileage()
    {
        using var client = await SignUpAsync("mr.updateadvance@example.com");
        var vehicle = await CreateVehicleAsync(client, currentMileage: 190_000);
        var record = await CreateRecordAsync(client, vehicle.Id, mileage: 188_000);

        await client.PutAsJsonAsync(
            $"{RecordsUrl(vehicle.Id)}/{record.Id}",
            new { type = 3, date = "2026-07-01", mileage = 205_000, cost = 300 });

        Assert.Equal(205_000, await CurrentMileageOfAsync(vehicle.Id));
    }

    [Fact]
    public async Task Delete_DoesNotPullBackTheVehiclesCurrentMileage()
    {
        using var client = await SignUpAsync("mr.deleteadvance@example.com");
        var vehicle = await CreateVehicleAsync(client, currentMileage: 190_000);
        var record = await CreateRecordAsync(client, vehicle.Id, mileage: 200_000);
        Assert.Equal(200_000, await CurrentMileageOfAsync(vehicle.Id));

        await client.DeleteAsync($"{RecordsUrl(vehicle.Id)}/{record.Id}");

        Assert.Equal(200_000, await CurrentMileageOfAsync(vehicle.Id));
    }

    // ---------- read ----------

    [Fact]
    public async Task Get_ReturnsTheRecord()
    {
        using var client = await SignUpAsync("mr.get@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var created = await CreateRecordAsync(client, vehicle.Id);

        var response = await client.GetAsync($"{RecordsUrl(vehicle.Id)}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var record = await response.Content.ReadFromJsonAsync<MaintenanceRecordResponse>();
        Assert.Equal(created.Id, record!.Id);
    }

    [Fact]
    public async Task Get_WithAnUnknownRecordId_ReturnsNotFound()
    {
        using var client = await SignUpAsync("mr.getunknown@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.GetAsync($"{RecordsUrl(vehicle.Id)}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_WithNoRecords_ReturnsEmpty()
    {
        using var client = await SignUpAsync("mr.empty@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var records = await client.GetFromJsonAsync<List<MaintenanceRecordResponse>>(RecordsUrl(vehicle.Id));

        Assert.Empty(records!);
    }

    [Fact]
    public async Task List_ForAnUnknownVehicle_ReturnsNotFound()
    {
        using var client = await SignUpAsync("mr.listunknown@example.com");

        var response = await client.GetAsync(RecordsUrl(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsRecordsMostRecentServiceFirst()
    {
        using var client = await SignUpAsync("mr.order@example.com");
        var vehicle = await CreateVehicleAsync(client, currentMileage: 300_000);

        await client.PostAsJsonAsync(RecordsUrl(vehicle.Id), new { type = 1, date = "2025-01-10", mileage = 160_000, cost = 50 });
        await client.PostAsJsonAsync(RecordsUrl(vehicle.Id), new { type = 1, date = "2026-05-20", mileage = 210_000, cost = 50 });
        await client.PostAsJsonAsync(RecordsUrl(vehicle.Id), new { type = 1, date = "2025-09-01", mileage = 185_000, cost = 50 });

        var records = await client.GetFromJsonAsync<List<MaintenanceRecordResponse>>(RecordsUrl(vehicle.Id));

        Assert.Equal(
            [new DateOnly(2026, 5, 20), new DateOnly(2025, 9, 1), new DateOnly(2025, 1, 10)],
            records!.Select(r => r.Date));
    }

    // ---------- update / delete ----------

    [Fact]
    public async Task Update_ReturnsTheUpdatedRecordAndPersistsIt()
    {
        using var client = await SignUpAsync("mr.update@example.com");
        var vehicle = await CreateVehicleAsync(client, currentMileage: 300_000);
        var record = await CreateRecordAsync(client, vehicle.Id, mileage: 195_000);

        var response = await client.PutAsJsonAsync(
            $"{RecordsUrl(vehicle.Id)}/{record.Id}",
            new { type = 3, date = "2026-07-01", mileage = 205_000, cost = 300, notes = "Front pads" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<MaintenanceRecordResponse>();
        Assert.Equal(MaintenanceType.BrakeService, updated!.Type);
        Assert.Equal(205_000, updated.Mileage);
        Assert.Equal("Front pads", updated.Notes);
    }

    [Fact]
    public async Task Update_ForAnUnknownRecord_ReturnsNotFound()
    {
        using var client = await SignUpAsync("mr.updateunknown@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PutAsJsonAsync(
            $"{RecordsUrl(vehicle.Id)}/{Guid.NewGuid()}",
            new { type = 1, date = "2026-06-01", mileage = 195_000, cost = 10 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContentAndTheRecordIsGone()
    {
        using var client = await SignUpAsync("mr.delete@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var record = await CreateRecordAsync(client, vehicle.Id);

        var response = await client.DeleteAsync($"{RecordsUrl(vehicle.Id)}/{record.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var followUp = await client.GetAsync($"{RecordsUrl(vehicle.Id)}/{record.Id}");
        Assert.Equal(HttpStatusCode.NotFound, followUp.StatusCode);
    }

    // ---------- isolation between users ----------

    [Fact]
    public async Task Get_AnotherUsersRecord_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("mr.alice.get@example.com");
        using var bob = await SignUpAsync("mr.bob.get@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        var aliceRecord = await CreateRecordAsync(alice, aliceVehicle.Id);

        var response = await bob.GetAsync($"{RecordsUrl(aliceVehicle.Id)}/{aliceRecord.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_ForAnotherUsersVehicle_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("mr.alice.list@example.com");
        using var bob = await SignUpAsync("mr.bob.list@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        await CreateRecordAsync(alice, aliceVehicle.Id);

        var response = await bob.GetAsync(RecordsUrl(aliceVehicle.Id));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_AnotherUsersRecord_ReturnsNotFoundAndChangesNothing()
    {
        using var alice = await SignUpAsync("mr.alice.update@example.com");
        using var bob = await SignUpAsync("mr.bob.update@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        var aliceRecord = await CreateRecordAsync(alice, aliceVehicle.Id, mileage: 195_000);

        var response = await bob.PutAsJsonAsync(
            $"{RecordsUrl(aliceVehicle.Id)}/{aliceRecord.Id}",
            new { type = 3, date = "2026-07-01", mileage = 205_000, cost = 300 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await db.MaintenanceRecords.SingleAsync(r => r.Id == aliceRecord.Id);
        Assert.Equal(195_000, stored.Mileage);
    }

    [Fact]
    public async Task Delete_AnotherUsersRecord_ReturnsNotFoundAndLeavesItAlone()
    {
        using var alice = await SignUpAsync("mr.alice.delete@example.com");
        using var bob = await SignUpAsync("mr.bob.delete@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        var aliceRecord = await CreateRecordAsync(alice, aliceVehicle.Id);

        var response = await bob.DeleteAsync($"{RecordsUrl(aliceVehicle.Id)}/{aliceRecord.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await db.MaintenanceRecords.AnyAsync(r => r.Id == aliceRecord.Id));
    }
}
