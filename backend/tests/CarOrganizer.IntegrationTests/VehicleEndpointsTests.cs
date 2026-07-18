using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// End-to-end tests for /api/vehicles over the full HTTP pipeline. Clients are authenticated by
/// actually registering and logging in rather than by forging a token, so the OwnerId written to
/// the database belongs to a real user — the same thing the Vehicles→AspNetUsers foreign key
/// demands in production (the in-memory provider would not enforce it for us).
/// A fresh factory (and in-memory database) is created per test to keep them isolated.
/// </summary>
public class VehicleEndpointsTests : IDisposable
{
    private const string VehiclesUrl = "/api/vehicles";
    private const string Password = "Passw0rd!23";

    private readonly CustomWebApplicationFactory _factory;

    public VehicleEndpointsTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Registers a user, logs them in, and returns a client that carries their token.</summary>
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

    private static object SampleVehicle(string model = "A4") =>
        new
        {
            make = "Audi",
            model,
            year = 2015,
            purchaseMileage = 150_000,
            currentMileage = 190_000,
            vin = "WAUZZZ8K1FA123456",
            registrationPlate = "CB1234AB",
            engine = "2.0 TDI",
        };

    private static async Task<VehicleResponse> CreateVehicleAsync(HttpClient client, string model = "A4")
    {
        var response = await client.PostAsJsonAsync(VehiclesUrl, SampleVehicle(model));
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>())!;
    }

    // ---------- authentication ----------

    [Fact]
    public async Task List_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(VehiclesUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(VehiclesUrl, SampleVehicle());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"{VehiclesUrl}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- create ----------

    [Fact]
    public async Task Create_WithAValidPayload_ReturnsCreated()
    {
        using var client = await SignUpAsync("create@example.com");

        var response = await client.PostAsJsonAsync(VehiclesUrl, SampleVehicle());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_PointsLocationAtTheNewVehicle()
    {
        using var client = await SignUpAsync("location@example.com");

        var response = await client.PostAsJsonAsync(VehiclesUrl, SampleVehicle());

        Assert.NotNull(response.Headers.Location);
        var followUp = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, followUp.StatusCode);
    }

    [Fact]
    public async Task Create_PersistsTheVehicleOwnedByTheCaller()
    {
        using var client = await SignUpAsync("owner@example.com");

        await client.PostAsJsonAsync(VehiclesUrl, SampleVehicle());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == "owner@example.com");
        var vehicle = await db.Vehicles.SingleAsync();

        Assert.Equal(user.Id, vehicle.OwnerId);
        Assert.Equal("Audi", vehicle.Make);
        Assert.Equal(150_000, vehicle.PurchaseMileage);
        Assert.Equal(190_000, vehicle.CurrentMileage);
    }

    [Fact]
    public async Task Create_EchoesTheVehicleBackWithAnId()
    {
        using var client = await SignUpAsync("echo@example.com");

        var vehicle = await CreateVehicleAsync(client);

        Assert.NotEqual(Guid.Empty, vehicle.Id);
        Assert.Equal("Audi", vehicle.Make);
        Assert.Equal("2.0 TDI", vehicle.Engine);
    }

    [Fact]
    public async Task Create_DoesNotLeakTheOwnerId()
    {
        using var client = await SignUpAsync("noleak@example.com");

        var response = await client.PostAsJsonAsync(VehiclesUrl, SampleVehicle());
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("ownerId", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WithoutAMake_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("nomake@example.com");

        var response = await client.PostAsJsonAsync(
            VehiclesUrl, new { model = "A4", year = 2015, purchaseMileage = 1000 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAnImplausibleYear_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("badyear@example.com");

        var response = await client.PostAsJsonAsync(
            VehiclesUrl, new { make = "Audi", model = "A4", year = 1750, purchaseMileage = 1000 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithNegativeMileage_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("badmileage@example.com");

        var response = await client.PostAsJsonAsync(
            VehiclesUrl, new { make = "Audi", model = "A4", year = 2015, purchaseMileage = -1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithCurrentBelowPurchase_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("mileageorder@example.com");

        // The odometer can't read lower now than when the car was bought.
        var response = await client.PostAsJsonAsync(
            VehiclesUrl, new { make = "Audi", model = "A4", year = 2015, purchaseMileage = 150_000, currentMileage = 140_000 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAnOverlongVin_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("badvin@example.com");

        var response = await client.PostAsJsonAsync(
            VehiclesUrl, new { make = "Audi", model = "A4", year = 2015, purchaseMileage = 1000, vin = new string('X', 18) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutTheOptionalFields_Succeeds()
    {
        using var client = await SignUpAsync("minimal@example.com");

        var response = await client.PostAsJsonAsync(
            VehiclesUrl, new { make = "Dacia", model = "Logan", year = 2010, purchaseMileage = 120_000 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.Null(vehicle!.Vin);
        Assert.Null(vehicle.Engine);
        // Current reading defaults to the purchase reading when omitted.
        Assert.Equal(120_000, vehicle.PurchaseMileage);
        Assert.Equal(120_000, vehicle.CurrentMileage);
    }

    // ---------- read ----------

    [Fact]
    public async Task Get_ReturnsTheVehicle()
    {
        using var client = await SignUpAsync("get@example.com");
        var created = await CreateVehicleAsync(client);

        var response = await client.GetAsync($"{VehiclesUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.Equal(created.Id, vehicle!.Id);
    }

    [Fact]
    public async Task Get_WithAnUnknownId_ReturnsNotFound()
    {
        using var client = await SignUpAsync("unknown@example.com");

        var response = await client.GetAsync($"{VehiclesUrl}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAMalformedId_ReturnsNotFound()
    {
        using var client = await SignUpAsync("malformed@example.com");

        // The {id:guid} route constraint rejects it before the action runs.
        var response = await client.GetAsync($"{VehiclesUrl}/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_WithAnEmptyGarage_ReturnsEmpty()
    {
        using var client = await SignUpAsync("emptygarage@example.com");

        var vehicles = await client.GetFromJsonAsync<List<VehicleResponse>>(VehiclesUrl);

        Assert.Empty(vehicles!);
    }

    [Fact]
    public async Task List_ReturnsEveryVehicleInTheGarage()
    {
        using var client = await SignUpAsync("garage@example.com");
        await CreateVehicleAsync(client, "A4");
        await CreateVehicleAsync(client, "A6");

        var vehicles = await client.GetFromJsonAsync<List<VehicleResponse>>(VehiclesUrl);

        Assert.Equal(2, vehicles!.Count);
        Assert.Contains(vehicles, v => v.Model == "A4");
        Assert.Contains(vehicles, v => v.Model == "A6");
    }

    [Fact]
    public async Task List_ReturnsTheNewestVehicleFirst()
    {
        using var client = await SignUpAsync("ordered@example.com");
        await CreateVehicleAsync(client, "oldest");
        await CreateVehicleAsync(client, "middle");
        await CreateVehicleAsync(client, "newest");

        var vehicles = await client.GetFromJsonAsync<List<VehicleResponse>>(VehiclesUrl);

        Assert.Equal(["newest", "middle", "oldest"], vehicles!.Select(v => v.Model));
    }

    // ---------- update ----------

    [Fact]
    public async Task Update_ReturnsTheUpdatedVehicle()
    {
        using var client = await SignUpAsync("update@example.com");
        var created = await CreateVehicleAsync(client);

        var response = await client.PutAsJsonAsync(
            $"{VehiclesUrl}/{created.Id}",
            new { make = "Audi", model = "A6", year = 2016, purchaseMileage = 150_000, currentMileage = 200_000, engine = "3.0 TDI" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.Equal("A6", vehicle!.Model);
        Assert.Equal(200_000, vehicle.CurrentMileage);
    }

    [Fact]
    public async Task Update_PersistsTheChange()
    {
        using var client = await SignUpAsync("persistupdate@example.com");
        var created = await CreateVehicleAsync(client);

        await client.PutAsJsonAsync(
            $"{VehiclesUrl}/{created.Id}",
            new { make = "Audi", model = "A6", year = 2016, purchaseMileage = 150_000, currentMileage = 200_000 });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var vehicle = await db.Vehicles.SingleAsync(v => v.Id == created.Id);

        Assert.Equal("A6", vehicle.Model);
        Assert.Equal(200_000, vehicle.CurrentMileage);
    }

    [Fact]
    public async Task Update_StampsUpdatedAt()
    {
        using var client = await SignUpAsync("stamp@example.com");
        var created = await CreateVehicleAsync(client);
        Assert.Null(created.UpdatedAtUtc);

        var response = await client.PutAsJsonAsync(
            $"{VehiclesUrl}/{created.Id}",
            new { make = "Audi", model = "A6", year = 2016, purchaseMileage = 150_000, currentMileage = 200_000 });

        var vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.NotNull(vehicle!.UpdatedAtUtc);
    }

    [Fact]
    public async Task Update_WithAnUnknownId_ReturnsNotFound()
    {
        using var client = await SignUpAsync("updateunknown@example.com");

        var response = await client.PutAsJsonAsync(
            $"{VehiclesUrl}/{Guid.NewGuid()}", new { make = "Audi", model = "A6", year = 2016, purchaseMileage = 1, currentMileage = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithAnInvalidPayload_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("updateinvalid@example.com");
        var created = await CreateVehicleAsync(client);

        var response = await client.PutAsJsonAsync(
            $"{VehiclesUrl}/{created.Id}", new { model = "A6", year = 2016, purchaseMileage = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- delete ----------

    [Fact]
    public async Task Delete_ReturnsNoContentAndTheVehicleIsGone()
    {
        using var client = await SignUpAsync("delete@example.com");
        var created = await CreateVehicleAsync(client);

        var response = await client.DeleteAsync($"{VehiclesUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var followUp = await client.GetAsync($"{VehiclesUrl}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, followUp.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesTheRow()
    {
        using var client = await SignUpAsync("deleterow@example.com");
        var created = await CreateVehicleAsync(client);

        await client.DeleteAsync($"{VehiclesUrl}/{created.Id}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.Vehicles.AnyAsync(v => v.Id == created.Id));
    }

    [Fact]
    public async Task Delete_Twice_ReturnsNotFoundTheSecondTime()
    {
        using var client = await SignUpAsync("deletetwice@example.com");
        var created = await CreateVehicleAsync(client);

        await client.DeleteAsync($"{VehiclesUrl}/{created.Id}");
        var second = await client.DeleteAsync($"{VehiclesUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }

    // ---------- isolation between users ----------
    // The point of these: one user must not be able to see, edit or delete another's vehicle, and
    // must not be able to tell the difference between "not yours" and "does not exist".

    [Fact]
    public async Task List_ShowsOnlyTheCallersOwnVehicles()
    {
        using var alice = await SignUpAsync("alice@example.com");
        using var bob = await SignUpAsync("bob@example.com");
        await CreateVehicleAsync(alice, "Alice's A4");
        await CreateVehicleAsync(bob, "Bob's A6");

        var aliceGarage = await alice.GetFromJsonAsync<List<VehicleResponse>>(VehiclesUrl);

        var vehicle = Assert.Single(aliceGarage!);
        Assert.Equal("Alice's A4", vehicle.Model);
    }

    [Fact]
    public async Task Get_AnotherUsersVehicle_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("alice.get@example.com");
        using var bob = await SignUpAsync("bob.get@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);

        var response = await bob.GetAsync($"{VehiclesUrl}/{aliceVehicle.Id}");

        // 404, not 403: an existing id must be indistinguishable from a made-up one.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_AnotherUsersVehicle_ReturnsNotFoundAndChangesNothing()
    {
        using var alice = await SignUpAsync("alice.update@example.com");
        using var bob = await SignUpAsync("bob.update@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);

        var response = await bob.PutAsJsonAsync(
            $"{VehiclesUrl}/{aliceVehicle.Id}",
            new { make = "Bob", model = "Stolen", year = 2020, purchaseMileage = 0, currentMileage = 0 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await db.Vehicles.SingleAsync(v => v.Id == aliceVehicle.Id);
        Assert.Equal("A4", stored.Model);
    }

    [Fact]
    public async Task Delete_AnotherUsersVehicle_ReturnsNotFoundAndLeavesItAlone()
    {
        using var alice = await SignUpAsync("alice.delete@example.com");
        using var bob = await SignUpAsync("bob.delete@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);

        var response = await bob.DeleteAsync($"{VehiclesUrl}/{aliceVehicle.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await db.Vehicles.AnyAsync(v => v.Id == aliceVehicle.Id));
    }

    [Fact]
    public async Task Get_AnotherUsersVehicle_IsIndistinguishableFromAnUnknownId()
    {
        using var alice = await SignUpAsync("alice.probe@example.com");
        using var bob = await SignUpAsync("bob.probe@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);

        var existsButNotBobs = await bob.GetAsync($"{VehiclesUrl}/{aliceVehicle.Id}");
        var doesNotExist = await bob.GetAsync($"{VehiclesUrl}/{Guid.NewGuid()}");

        Assert.Equal(doesNotExist.StatusCode, existsButNotBobs.StatusCode);
        Assert.Equal(
            await BodyWithoutTraceIdAsync(doesNotExist),
            await BodyWithoutTraceIdAsync(existsButNotBobs));
    }

    /// <summary>
    /// The response body minus <c>traceId</c>, which ASP.NET stamps on every ProblemDetails with a
    /// fresh per-request correlation id. It differs by design and says nothing about the vehicle,
    /// so it is the one field that may legitimately vary between two otherwise identical 404s.
    /// </summary>
    private static async Task<string> BodyWithoutTraceIdAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (JsonNode.Parse(body) is not JsonObject problemDetails)
        {
            return body;
        }

        problemDetails.Remove("traceId");

        return problemDetails.ToJsonString();
    }
}
