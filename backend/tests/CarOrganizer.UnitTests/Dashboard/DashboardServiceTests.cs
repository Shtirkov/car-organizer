using CarOrganizer.Application.Dashboard;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Domain.Enums;
using CarOrganizer.Infrastructure.Dashboard;
using Moq;

namespace CarOrganizer.UnitTests.Dashboard;

/// <summary>
/// Covers <see cref="DashboardService"/> against mocked stores: splitting the obligations due inside
/// the horizon into the overdue and expiring buckets, the day arithmetic on each, the ordering the
/// screen depends on, and hanging everything off the right vehicle.
/// </summary>
public class DashboardServiceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    /// <summary>The same "today" the service computes — both read the UTC date.</summary>
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly Mock<IVehicleStore> _vehicles = new();
    private readonly Mock<IVehicleObligationStore> _obligations = new();
    private readonly Mock<IMaintenanceRecordStore> _records = new();
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _vehicles
            .Setup(v => v.ListByOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _obligations
            .Setup(o => o.ListByOwnerDueByAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _records
            .Setup(r => r.ListRecentByVehicleAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _sut = new DashboardService(_vehicles.Object, _obligations.Object, _records.Object);
    }

    private static Vehicle SampleVehicle(Guid? id = null, string make = "Audi") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            OwnerId = OwnerId,
            Make = make,
            Model = "A4",
            Year = 2015,
            RegistrationPlate = "CA1234AB",
            PurchaseMileage = 150_000,
            CurrentMileage = 190_000,
        };

    private void GarageOf(params Vehicle[] vehicles) =>
        _vehicles
            .Setup(v => v.ListByOwnerAsync(OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicles);

    /// <summary>The store hands these back already ordered by ValidUntil ascending.</summary>
    private void ObligationsDue(params VehicleObligation[] obligations) =>
        _obligations
            .Setup(o => o.ListByOwnerDueByAsync(OwnerId, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(obligations.OrderBy(o => o.ValidUntil).ToArray());

    private static VehicleObligation ObligationOn(
        Guid vehicleId, DateOnly validUntil, ObligationType type = ObligationType.Insurance) =>
        new()
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Type = type,
            ValidUntil = validUntil,
            Provider = "Бул Инс",
            PolicyNumber = "BG/03/123456789",
        };

    private Task<DashboardResponse> RunAsync(int withinDays = 30, int recentCount = 5) =>
        _sut.GetAsync(OwnerId, withinDays, recentCount, CancellationToken.None);

    // ---------- shape ----------

    [Fact]
    public async Task GetAsync_WithNoVehicles_ReturnsAnEmptyGarageRatherThanNothing()
    {
        var dashboard = await RunAsync();

        Assert.Equal(0, dashboard.VehicleCount);
        Assert.Empty(dashboard.Vehicles);
    }

    [Fact]
    public async Task GetAsync_EchoesTheHorizonAndCountsTheGarage()
    {
        GarageOf(SampleVehicle(), SampleVehicle(make: "VW"));

        var dashboard = await RunAsync(withinDays: 45);

        Assert.Equal(45, dashboard.WithinDays);
        Assert.Equal(2, dashboard.VehicleCount);
        Assert.Equal(2, dashboard.Vehicles.Count);
    }

    [Fact]
    public async Task GetAsync_MapsTheVehiclesOwnFields()
    {
        var vehicle = SampleVehicle();
        GarageOf(vehicle);

        var block = (await RunAsync()).Vehicles.Single();

        Assert.Equal(vehicle.Id, block.Id);
        Assert.Equal("Audi", block.Make);
        Assert.Equal("A4", block.Model);
        Assert.Equal(2015, block.Year);
        Assert.Equal("CA1234AB", block.RegistrationPlate);
        Assert.Equal(190_000, block.CurrentMileage);
    }

    // ---------- horizon ----------

    [Fact]
    public async Task GetAsync_AsksTheStoreForEverythingDueUpToTheHorizon()
    {
        await RunAsync(withinDays: 30);

        // Far-future renewals are excluded in the database, not filtered out afterwards.
        _obligations.Verify(
            o => o.ListByOwnerDueByAsync(OwnerId, Today.AddDays(30), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------- bucketing ----------

    [Fact]
    public async Task GetAsync_PutsAPastRenewalInTheOverdueBucketWithItsDayCount()
    {
        var vehicle = SampleVehicle();
        GarageOf(vehicle);
        ObligationsDue(ObligationOn(vehicle.Id, Today.AddDays(-34)));

        var block = (await RunAsync()).Vehicles.Single();

        var overdue = Assert.Single(block.OverdueObligations);
        Assert.Equal(34, overdue.DaysOverdue);
        Assert.Equal(Today.AddDays(-34), overdue.ValidUntil);
        Assert.Equal("Бул Инс", overdue.Provider);
        Assert.Empty(block.ExpiringObligations);
    }

    [Fact]
    public async Task GetAsync_PutsAComingRenewalInTheExpiringBucketWithItsDayCount()
    {
        var vehicle = SampleVehicle();
        GarageOf(vehicle);
        ObligationsDue(ObligationOn(vehicle.Id, Today.AddDays(16)));

        var block = (await RunAsync()).Vehicles.Single();

        var expiring = Assert.Single(block.ExpiringObligations);
        Assert.Equal(16, expiring.DaysRemaining);
        Assert.Empty(block.OverdueObligations);
    }

    [Fact]
    public async Task GetAsync_TreatsSomethingExpiringTodayAsStillExpiringNotOverdue()
    {
        var vehicle = SampleVehicle();
        GarageOf(vehicle);
        ObligationsDue(ObligationOn(vehicle.Id, Today));

        var block = (await RunAsync()).Vehicles.Single();

        // You can still renew it today, so it is not yet a failure.
        Assert.Empty(block.OverdueObligations);
        Assert.Equal(0, Assert.Single(block.ExpiringObligations).DaysRemaining);
    }

    [Fact]
    public async Task GetAsync_YesterdayIsOverdueByOneDay()
    {
        var vehicle = SampleVehicle();
        GarageOf(vehicle);
        ObligationsDue(ObligationOn(vehicle.Id, Today.AddDays(-1)));

        var block = (await RunAsync()).Vehicles.Single();

        Assert.Equal(1, Assert.Single(block.OverdueObligations).DaysOverdue);
    }

    // ---------- ordering ----------

    [Fact]
    public async Task GetAsync_PutsTheMostOverdueFirstAndTheSoonestRenewalFirst()
    {
        var vehicle = SampleVehicle();
        GarageOf(vehicle);
        ObligationsDue(
            ObligationOn(vehicle.Id, Today.AddDays(20), ObligationType.Casco),
            ObligationOn(vehicle.Id, Today.AddDays(-3), ObligationType.Vignette),
            ObligationOn(vehicle.Id, Today.AddDays(5), ObligationType.Tax),
            ObligationOn(vehicle.Id, Today.AddDays(-40), ObligationType.Insurance));

        var block = (await RunAsync()).Vehicles.Single();

        // Most urgent first within each bucket: the longest-overdue, then the nearest renewal.
        Assert.Equal([40, 3], block.OverdueObligations.Select(o => o.DaysOverdue));
        Assert.Equal([5, 20], block.ExpiringObligations.Select(o => o.DaysRemaining));
    }

    // ---------- grouping ----------

    [Fact]
    public async Task GetAsync_HangsEachObligationOffItsOwnVehicle()
    {
        var audi = SampleVehicle(make: "Audi");
        var vw = SampleVehicle(make: "VW");
        GarageOf(audi, vw);
        ObligationsDue(
            ObligationOn(audi.Id, Today.AddDays(3), ObligationType.Insurance),
            ObligationOn(vw.Id, Today.AddDays(-7), ObligationType.Vignette),
            ObligationOn(vw.Id, Today.AddDays(9), ObligationType.Tax));

        var dashboard = await RunAsync();

        var audiBlock = dashboard.Vehicles.Single(v => v.Id == audi.Id);
        var vwBlock = dashboard.Vehicles.Single(v => v.Id == vw.Id);
        Assert.Equal(ObligationType.Insurance, Assert.Single(audiBlock.ExpiringObligations).Type);
        Assert.Empty(audiBlock.OverdueObligations);
        Assert.Equal(ObligationType.Vignette, Assert.Single(vwBlock.OverdueObligations).Type);
        Assert.Equal(ObligationType.Tax, Assert.Single(vwBlock.ExpiringObligations).Type);
    }

    [Fact]
    public async Task GetAsync_LeavesAVehicleWithNothingDueWithEmptyBuckets()
    {
        var vehicle = SampleVehicle();
        GarageOf(vehicle);

        var block = (await RunAsync()).Vehicles.Single();

        Assert.Empty(block.OverdueObligations);
        Assert.Empty(block.ExpiringObligations);
        Assert.Empty(block.RecentMaintenance);
    }

    // ---------- recent maintenance ----------

    [Fact]
    public async Task GetAsync_AsksForTheRequestedNumberOfRecentServicesPerVehicle()
    {
        var audi = SampleVehicle(make: "Audi");
        var vw = SampleVehicle(make: "VW");
        GarageOf(audi, vw);

        await RunAsync(recentCount: 3);

        // The cap is per vehicle, not shared across the garage.
        _records.Verify(r => r.ListRecentByVehicleAsync(audi.Id, 3, It.IsAny<CancellationToken>()), Times.Once);
        _records.Verify(r => r.ListRecentByVehicleAsync(vw.Id, 3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_MapsRecentMaintenanceToItsSummaryRow()
    {
        var vehicle = SampleVehicle();
        GarageOf(vehicle);
        var record = new MaintenanceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Type = MaintenanceType.OilChange,
            Date = Today.AddDays(-60),
            Mileage = 195_000,
            Cost = 120.50m,
        };
        _records
            .Setup(r => r.ListRecentByVehicleAsync(vehicle.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([record]);

        var block = (await RunAsync()).Vehicles.Single();

        var summary = Assert.Single(block.RecentMaintenance);
        Assert.Equal(record.Id, summary.Id);
        Assert.Equal(MaintenanceType.OilChange, summary.Type);
        Assert.Equal(Today.AddDays(-60), summary.Date);
        Assert.Equal(195_000, summary.Mileage);
        Assert.Equal(120.50m, summary.Cost);
    }
}
