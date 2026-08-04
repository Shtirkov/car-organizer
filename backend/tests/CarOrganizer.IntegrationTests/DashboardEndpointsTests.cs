using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Dashboard;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Domain.Enums;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// End-to-end tests for /api/dashboard over the full HTTP pipeline. Clients register and log in for
/// real, then build a garage through the public endpoints, so the dashboard is assembled from the
/// same rows the rest of the API writes.
/// </summary>
public class DashboardEndpointsTests : IDisposable
{
    private const string Password = "Passw0rd!23";

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CustomWebApplicationFactory _factory;

    public DashboardEndpointsTests()
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

    private static async Task<VehicleResponse> CreateVehicleAsync(HttpClient client, string make = "Audi")
    {
        var response = await client.PostAsJsonAsync(
            "/api/vehicles",
            new { make, model = "A4", year = 2015, registrationPlate = "CA1234AB", purchaseMileage = 150_000, currentMileage = 190_000 });
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>())!;
    }

    private static async Task CreateObligationAsync(
        HttpClient client, Guid vehicleId, DateOnly validUntil, ObligationType type = ObligationType.Insurance)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/vehicles/{vehicleId}/obligations",
            new { type = (int)type, validUntil = validUntil.ToString("yyyy-MM-dd"), cost = 450.00 });
        response.EnsureSuccessStatusCode();
    }

    private static async Task CreateRecordAsync(HttpClient client, Guid vehicleId, DateOnly date, int mileage)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/vehicles/{vehicleId}/maintenance-records",
            new { type = (int)MaintenanceType.OilChange, date = date.ToString("yyyy-MM-dd"), mileage, cost = 120.50 });
        response.EnsureSuccessStatusCode();
    }

    private static async Task<DashboardResponse> DashboardAsync(HttpClient client, string query = "")
    {
        var response = await client.GetAsync("/api/dashboard" + query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DashboardResponse>())!;
    }

    // ---------- authentication ----------

    [Fact]
    public async Task Get_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- empty garage ----------

    [Fact]
    public async Task Get_WithNoVehicles_ReturnsAnEmptyGarageNotANotFound()
    {
        using var client = await SignUpAsync("dash.empty@example.com");

        var dashboard = await DashboardAsync(client);

        Assert.Equal(0, dashboard.VehicleCount);
        Assert.Empty(dashboard.Vehicles);
    }

    // ---------- buckets ----------

    [Fact]
    public async Task Get_SplitsRenewalsIntoOverdueAndExpiring()
    {
        using var client = await SignUpAsync("dash.buckets@example.com");
        var vehicle = await CreateVehicleAsync(client);
        await CreateObligationAsync(client, vehicle.Id, Today.AddDays(-10), ObligationType.Insurance);
        await CreateObligationAsync(client, vehicle.Id, Today.AddDays(12), ObligationType.Vignette);

        var block = (await DashboardAsync(client)).Vehicles.Single();

        Assert.Equal(10, Assert.Single(block.OverdueObligations).DaysOverdue);
        Assert.Equal(12, Assert.Single(block.ExpiringObligations).DaysRemaining);
    }

    [Fact]
    public async Task Get_OrdersTheMostUrgentFirstInEachBucket()
    {
        using var client = await SignUpAsync("dash.order@example.com");
        var vehicle = await CreateVehicleAsync(client);
        await CreateObligationAsync(client, vehicle.Id, Today.AddDays(-3), ObligationType.Vignette);
        await CreateObligationAsync(client, vehicle.Id, Today.AddDays(20), ObligationType.Casco);
        await CreateObligationAsync(client, vehicle.Id, Today.AddDays(-40), ObligationType.Insurance);
        await CreateObligationAsync(client, vehicle.Id, Today.AddDays(5), ObligationType.Tax);

        var block = (await DashboardAsync(client)).Vehicles.Single();

        Assert.Equal([40, 3], block.OverdueObligations.Select(o => o.DaysOverdue));
        Assert.Equal([5, 20], block.ExpiringObligations.Select(o => o.DaysRemaining));
    }

    [Fact]
    public async Task Get_ExcludesRenewalsBeyondTheHorizon()
    {
        using var client = await SignUpAsync("dash.horizon@example.com");
        var vehicle = await CreateVehicleAsync(client);
        await CreateObligationAsync(client, vehicle.Id, Today.AddDays(200), ObligationType.Casco);

        var withDefault = (await DashboardAsync(client)).Vehicles.Single();
        Assert.Empty(withDefault.ExpiringObligations);

        var widened = (await DashboardAsync(client, "?withinDays=365")).Vehicles.Single();
        Assert.Equal(200, Assert.Single(widened.ExpiringObligations).DaysRemaining);
    }

    [Fact]
    public async Task Get_ShowsOverdueRenewalsHoweverOldTheyAre()
    {
        using var client = await SignUpAsync("dash.oldoverdue@example.com");
        var vehicle = await CreateVehicleAsync(client);
        await CreateObligationAsync(client, vehicle.Id, Today.AddDays(-800), ObligationType.Insurance);

        var block = (await DashboardAsync(client)).Vehicles.Single();

        // A years-old lapse is still a problem; the horizon bounds the future, not the past.
        Assert.Equal(800, Assert.Single(block.OverdueObligations).DaysOverdue);
    }

    // ---------- grouping ----------

    [Fact]
    public async Task Get_GroupsEachVehiclesRenewalsUnderThatVehicle()
    {
        using var client = await SignUpAsync("dash.grouping@example.com");
        var audi = await CreateVehicleAsync(client, "Audi");
        var vw = await CreateVehicleAsync(client, "VW");
        await CreateObligationAsync(client, audi.Id, Today.AddDays(4), ObligationType.Insurance);
        await CreateObligationAsync(client, vw.Id, Today.AddDays(-6), ObligationType.Vignette);

        var dashboard = await DashboardAsync(client);

        Assert.Equal(2, dashboard.VehicleCount);
        var audiBlock = dashboard.Vehicles.Single(v => v.Id == audi.Id);
        var vwBlock = dashboard.Vehicles.Single(v => v.Id == vw.Id);
        Assert.Equal(ObligationType.Insurance, Assert.Single(audiBlock.ExpiringObligations).Type);
        Assert.Empty(audiBlock.OverdueObligations);
        Assert.Equal(ObligationType.Vignette, Assert.Single(vwBlock.OverdueObligations).Type);
        Assert.Empty(vwBlock.ExpiringObligations);
    }

    [Fact]
    public async Task Get_CarriesTheVehiclesOwnDetailsSoTheClientCanRenderThePicker()
    {
        using var client = await SignUpAsync("dash.vehicledetails@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var block = (await DashboardAsync(client)).Vehicles.Single();

        Assert.Equal(vehicle.Id, block.Id);
        Assert.Equal("Audi", block.Make);
        Assert.Equal("A4", block.Model);
        Assert.Equal(2015, block.Year);
        Assert.Equal("CA1234AB", block.RegistrationPlate);
        Assert.Equal(190_000, block.CurrentMileage);
    }

    // ---------- recent maintenance ----------

    [Fact]
    public async Task Get_ReturnsTheMostRecentServicesNewestFirst()
    {
        using var client = await SignUpAsync("dash.recent@example.com");
        var vehicle = await CreateVehicleAsync(client);
        await CreateRecordAsync(client, vehicle.Id, Today.AddDays(-90), 191_000);
        await CreateRecordAsync(client, vehicle.Id, Today.AddDays(-10), 195_000);
        await CreateRecordAsync(client, vehicle.Id, Today.AddDays(-45), 193_000);

        var block = (await DashboardAsync(client)).Vehicles.Single();

        Assert.Equal(
            [Today.AddDays(-10), Today.AddDays(-45), Today.AddDays(-90)],
            block.RecentMaintenance.Select(r => r.Date));
    }

    [Fact]
    public async Task Get_CapsRecentServicesPerVehicleNotAcrossTheGarage()
    {
        using var client = await SignUpAsync("dash.recentcap@example.com");
        var audi = await CreateVehicleAsync(client, "Audi");
        var vw = await CreateVehicleAsync(client, "VW");
        foreach (var offset in new[] { 10, 20, 30 })
        {
            await CreateRecordAsync(client, audi.Id, Today.AddDays(-offset), 190_000 + offset);
            await CreateRecordAsync(client, vw.Id, Today.AddDays(-offset), 190_000 + offset);
        }

        var dashboard = await DashboardAsync(client, "?recentCount=2");

        // Two each, not two shared between them.
        Assert.Equal(2, dashboard.Vehicles.Single(v => v.Id == audi.Id).RecentMaintenance.Count);
        Assert.Equal(2, dashboard.Vehicles.Single(v => v.Id == vw.Id).RecentMaintenance.Count);
    }

    // ---------- parameters ----------

    [Fact]
    public async Task Get_WithoutParameters_AppliesTheDefaultHorizon()
    {
        using var client = await SignUpAsync("dash.defaults@example.com");

        var dashboard = await DashboardAsync(client);

        Assert.Equal(DashboardLimits.DefaultWithinDays, dashboard.WithinDays);
    }

    [Fact]
    public async Task Get_EchoesTheHorizonItActuallyApplied()
    {
        using var client = await SignUpAsync("dash.echo@example.com");

        var dashboard = await DashboardAsync(client, "?withinDays=90");

        Assert.Equal(90, dashboard.WithinDays);
    }

    [Theory]
    [InlineData("?withinDays=0")]
    [InlineData("?withinDays=-1")]
    [InlineData("?withinDays=366")]
    [InlineData("?recentCount=0")]
    [InlineData("?recentCount=51")]
    public async Task Get_WithAnOutOfRangeParameter_ReturnsBadRequest(string query)
    {
        using var client = await SignUpAsync($"dash.range{query.GetHashCode()}@example.com");

        var response = await client.GetAsync("/api/dashboard" + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("?withinDays=1")]
    [InlineData("?withinDays=365")]
    [InlineData("?recentCount=1")]
    [InlineData("?recentCount=50")]
    public async Task Get_AtTheEdgesOfTheAllowedRange_IsAccepted(string query)
    {
        using var client = await SignUpAsync($"dash.edge{query.GetHashCode()}@example.com");

        var response = await client.GetAsync("/api/dashboard" + query);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------- isolation between users ----------

    [Fact]
    public async Task Get_ShowsOnlyTheCallersOwnGarage()
    {
        using var alice = await SignUpAsync("dash.alice@example.com");
        using var bob = await SignUpAsync("dash.bob@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice, "Audi");
        await CreateObligationAsync(alice, aliceVehicle.Id, Today.AddDays(-5));

        var bobDashboard = await DashboardAsync(bob);

        Assert.Equal(0, bobDashboard.VehicleCount);
        Assert.Empty(bobDashboard.Vehicles);
    }

    [Fact]
    public async Task Get_DoesNotLeakAnotherUsersRenewalsIntoTheCallersVehicle()
    {
        using var alice = await SignUpAsync("dash.alice.leak@example.com");
        using var bob = await SignUpAsync("dash.bob.leak@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice, "Audi");
        await CreateObligationAsync(alice, aliceVehicle.Id, Today.AddDays(-5));
        var bobVehicle = await CreateVehicleAsync(bob, "VW");

        var bobBlock = (await DashboardAsync(bob)).Vehicles.Single();

        Assert.Equal(bobVehicle.Id, bobBlock.Id);
        Assert.Empty(bobBlock.OverdueObligations);
        Assert.Empty(bobBlock.ExpiringObligations);
    }
}
