using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Infrastructure.Vehicles;
using Moq;

namespace CarOrganizer.UnitTests.Vehicles;

/// <summary>
/// Covers <see cref="VehicleService"/> against a mocked store: the entity/DTO mapping, and the
/// rule that every lookup is scoped to the owner.
/// </summary>
public class VehicleServiceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private readonly Mock<IVehicleStore> _store = new();
    private readonly VehicleService _sut;

    public VehicleServiceTests()
    {
        _sut = new VehicleService(_store.Object);
    }

    private static Vehicle SampleVehicle(Guid? ownerId = null) => new()
    {
        OwnerId = ownerId ?? OwnerId,
        Make = "Audi",
        Model = "A4",
        Year = 2015,
        PurchaseMileage = 150_000,
        CurrentMileage = 190_000,
        Vin = "WAUZZZ8K1FA123456",
        RegistrationPlate = "CB1234AB",
        Engine = "2.0 TDI",
    };

    private static CreateVehicleRequest SampleCreateRequest() =>
        new("Audi", "A4", 2015, 150_000, 190_000, "WAUZZZ8K1FA123456", "CB1234AB", "2.0 TDI");

    private static UpdateVehicleRequest SampleUpdateRequest() =>
        new("Audi", "A6", 2016, 150_000, 200_000, "WAUZZZ8K1FA654321", "CB4321BA", "3.0 TDI");

    [Fact]
    public async Task CreateAsync_PersistsTheVehicleForTheCallingOwner()
    {
        Vehicle? persisted = null;
        _store
            .Setup(s => s.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Callback<Vehicle, CancellationToken>((v, _) => persisted = v)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(OwnerId, SampleCreateRequest(), CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal(OwnerId, persisted!.OwnerId);
    }

    [Fact]
    public async Task CreateAsync_MapsEveryFieldFromTheRequest()
    {
        Vehicle? persisted = null;
        _store
            .Setup(s => s.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Callback<Vehicle, CancellationToken>((v, _) => persisted = v)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(OwnerId, SampleCreateRequest(), CancellationToken.None);

        Assert.Equal("Audi", persisted!.Make);
        Assert.Equal("A4", persisted.Model);
        Assert.Equal(2015, persisted.Year);
        Assert.Equal(150_000, persisted.PurchaseMileage);
        Assert.Equal(190_000, persisted.CurrentMileage);
        Assert.Equal("WAUZZZ8K1FA123456", persisted.Vin);
        Assert.Equal("CB1234AB", persisted.RegistrationPlate);
        Assert.Equal("2.0 TDI", persisted.Engine);
    }

    [Fact]
    public async Task CreateAsync_WithoutACurrentReading_DefaultsItToThePurchaseReading()
    {
        Vehicle? persisted = null;
        _store
            .Setup(s => s.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Callback<Vehicle, CancellationToken>((v, _) => persisted = v)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(
            OwnerId,
            new CreateVehicleRequest("Dacia", "Logan", 2010, 120_000, null, null, null, null),
            CancellationToken.None);

        Assert.Equal(120_000, persisted!.PurchaseMileage);
        Assert.Equal(120_000, persisted.CurrentMileage);
    }

    [Fact]
    public async Task CreateAsync_WithACurrentReading_KeepsItDistinctFromPurchase()
    {
        Vehicle? persisted = null;
        _store
            .Setup(s => s.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Callback<Vehicle, CancellationToken>((v, _) => persisted = v)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(
            OwnerId,
            new CreateVehicleRequest("Dacia", "Logan", 2010, 120_000, 175_000, null, null, null),
            CancellationToken.None);

        Assert.Equal(120_000, persisted!.PurchaseMileage);
        Assert.Equal(175_000, persisted.CurrentMileage);
    }

    [Fact]
    public async Task CreateAsync_ReturnsTheStoredVehicleWithItsGeneratedId()
    {
        _store
            .Setup(s => s.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _sut.CreateAsync(OwnerId, SampleCreateRequest(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Audi", response.Make);
    }

    [Fact]
    public async Task CreateAsync_WithNullOptionalFields_LeavesThemNull()
    {
        Vehicle? persisted = null;
        _store
            .Setup(s => s.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Callback<Vehicle, CancellationToken>((v, _) => persisted = v)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(
            OwnerId, new CreateVehicleRequest("Dacia", "Logan", 2010, 120_000, null, null, null, null), CancellationToken.None);

        Assert.Null(persisted!.Vin);
        Assert.Null(persisted.RegistrationPlate);
        Assert.Null(persisted.Engine);
    }

    [Fact]
    public async Task ListAsync_AsksTheStoreForTheCallersGarageOnly()
    {
        _store
            .Setup(s => s.ListByOwnerAsync(OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([SampleVehicle()]);

        await _sut.ListAsync(OwnerId, CancellationToken.None);

        _store.Verify(s => s.ListByOwnerAsync(OwnerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListAsync_MapsEveryVehicleToAResponse()
    {
        _store
            .Setup(s => s.ListByOwnerAsync(OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([SampleVehicle(), SampleVehicle()]);

        var responses = await _sut.ListAsync(OwnerId, CancellationToken.None);

        Assert.Equal(2, responses.Count);
        Assert.All(responses, r => Assert.Equal("Audi", r.Make));
    }

    [Fact]
    public async Task ListAsync_WithAnEmptyGarage_ReturnsEmpty()
    {
        _store
            .Setup(s => s.ListByOwnerAsync(OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var responses = await _sut.ListAsync(OwnerId, CancellationToken.None);

        Assert.Empty(responses);
    }

    [Fact]
    public async Task GetAsync_ScopesTheLookupToTheOwner()
    {
        var vehicleId = Guid.NewGuid();
        _store
            .Setup(s => s.FindByIdAsync(vehicleId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleVehicle());

        await _sut.GetAsync(OwnerId, vehicleId, CancellationToken.None);

        _store.Verify(s => s.FindByIdAsync(vehicleId, OwnerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenTheStoreFindsNothing_ReturnsNull()
    {
        _store
            .Setup(s => s.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var response = await _sut.GetAsync(OwnerId, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetAsync_MapsTheAuditTimestamps()
    {
        var vehicle = SampleVehicle();
        vehicle.UpdatedAtUtc = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        _store
            .Setup(s => s.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var response = await _sut.GetAsync(OwnerId, vehicle.Id, CancellationToken.None);

        Assert.Equal(vehicle.CreatedAtUtc, response!.CreatedAtUtc);
        Assert.Equal(vehicle.UpdatedAtUtc, response.UpdatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_WritesEveryEditableField()
    {
        var vehicle = SampleVehicle();
        _store
            .Setup(s => s.FindByIdAsync(vehicle.Id, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var response = await _sut.UpdateAsync(OwnerId, vehicle.Id, SampleUpdateRequest(), CancellationToken.None);

        Assert.Equal("A6", response!.Model);
        Assert.Equal(2016, response.Year);
        Assert.Equal(150_000, response.PurchaseMileage);
        Assert.Equal(200_000, response.CurrentMileage);
        Assert.Equal("WAUZZZ8K1FA654321", response.Vin);
        Assert.Equal("CB4321BA", response.RegistrationPlate);
        Assert.Equal("3.0 TDI", response.Engine);
    }

    [Fact]
    public async Task UpdateAsync_OmittingAnOptionalField_ClearsIt()
    {
        var vehicle = SampleVehicle();
        _store
            .Setup(s => s.FindByIdAsync(vehicle.Id, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var response = await _sut.UpdateAsync(
            OwnerId, vehicle.Id, new UpdateVehicleRequest("Audi", "A4", 2015, 150_000, 190_000, null, null, null), CancellationToken.None);

        Assert.Null(response!.Vin);
        Assert.Null(response.RegistrationPlate);
        Assert.Null(response.Engine);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotChangeTheOwner()
    {
        var vehicle = SampleVehicle();
        _store
            .Setup(s => s.FindByIdAsync(vehicle.Id, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        await _sut.UpdateAsync(OwnerId, vehicle.Id, SampleUpdateRequest(), CancellationToken.None);

        Assert.Equal(OwnerId, vehicle.OwnerId);
    }

    [Fact]
    public async Task UpdateAsync_PersistsThroughTheStore()
    {
        var vehicle = SampleVehicle();
        _store
            .Setup(s => s.FindByIdAsync(vehicle.Id, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        await _sut.UpdateAsync(OwnerId, vehicle.Id, SampleUpdateRequest(), CancellationToken.None);

        _store.Verify(s => s.UpdateAsync(vehicle, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTheStoreFindsNothing_ReturnsNullAndPersistsNothing()
    {
        _store
            .Setup(s => s.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var response = await _sut.UpdateAsync(OwnerId, Guid.NewGuid(), SampleUpdateRequest(), CancellationToken.None);

        Assert.Null(response);
        _store.Verify(s => s.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheVehicleAndReportsSuccess()
    {
        var vehicle = SampleVehicle();
        _store
            .Setup(s => s.FindByIdAsync(vehicle.Id, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var deleted = await _sut.DeleteAsync(OwnerId, vehicle.Id, CancellationToken.None);

        Assert.True(deleted);
        _store.Verify(s => s.RemoveAsync(vehicle, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTheStoreFindsNothing_ReportsFailureAndRemovesNothing()
    {
        _store
            .Setup(s => s.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var deleted = await _sut.DeleteAsync(OwnerId, Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
        _store.Verify(s => s.RemoveAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
