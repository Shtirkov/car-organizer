using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.MaintenanceRecords;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Domain.Enums;
using CarOrganizer.Infrastructure.MaintenanceRecords;
using Moq;

namespace CarOrganizer.UnitTests.MaintenanceRecords;

/// <summary>
/// Covers <see cref="MaintenanceRecordService"/> against mocked stores: the vehicle-ownership gate,
/// the entity/DTO mapping, and the mileage auto-advance rule.
/// </summary>
public class MaintenanceRecordServiceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid VehicleId = Guid.NewGuid();

    private readonly Mock<IMaintenanceRecordStore> _records = new();
    private readonly Mock<IVehicleStore> _vehicles = new();
    private readonly Mock<IDocumentStore> _documents = new();
    private readonly Mock<IFileStorage> _storage = new();
    private readonly MaintenanceRecordService _sut;

    public MaintenanceRecordServiceTests()
    {
        // Deleting sweeps the files of the documents the cascade removes; most tests have none.
        _documents
            .Setup(d => d.ListByMaintenanceRecordAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _sut = new MaintenanceRecordService(_records.Object, _vehicles.Object, _documents.Object, _storage.Object);
    }

    private static Vehicle VehicleWithCurrentMileage(int currentMileage) => new()
    {
        Id = VehicleId,
        OwnerId = OwnerId,
        Make = "Audi",
        Model = "A4",
        Year = 2015,
        PurchaseMileage = 150_000,
        CurrentMileage = currentMileage,
    };

    private void OwnsVehicle(Vehicle vehicle) =>
        _vehicles
            .Setup(v => v.FindByIdAsync(VehicleId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

    private void OwnsNoSuchVehicle() =>
        _vehicles
            .Setup(v => v.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

    private static CreateMaintenanceRecordRequest CreateRequest(int mileage = 195_000) =>
        new(MaintenanceType.OilChange, new DateOnly(2026, 6, 1), mileage, 120.50m, "Synthetic 5W-30");

    private static UpdateMaintenanceRecordRequest UpdateRequest(int mileage = 205_000) =>
        new(MaintenanceType.BrakeService, new DateOnly(2026, 7, 1), mileage, 300m, "Front pads");

    // ---------- ownership gate ----------

    [Fact]
    public async Task CreateAsync_WhenVehicleIsNotTheOwners_ReturnsNullAndPersistsNothing()
    {
        OwnsNoSuchVehicle();

        var response = await _sut.CreateAsync(OwnerId, VehicleId, CreateRequest(), CancellationToken.None);

        Assert.Null(response);
        _records.Verify(r => r.AddAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_WhenVehicleIsNotTheOwners_ReturnsNull()
    {
        OwnsNoSuchVehicle();

        var response = await _sut.ListAsync(OwnerId, VehicleId, CancellationToken.None);

        Assert.Null(response);
        _records.Verify(r => r.ListByVehicleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_WhenVehicleIsNotTheOwners_ReturnsNull()
    {
        OwnsNoSuchVehicle();

        var response = await _sut.GetAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task DeleteAsync_WhenVehicleIsNotTheOwners_ReturnsFalse()
    {
        OwnsNoSuchVehicle();

        var deleted = await _sut.DeleteAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
        _records.Verify(r => r.RemoveAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- create + mapping ----------

    [Fact]
    public async Task CreateAsync_MapsEveryFieldAndScopesToTheVehicle()
    {
        OwnsVehicle(VehicleWithCurrentMileage(190_000));
        MaintenanceRecord? captured = null;
        _records
            .Setup(r => r.AddAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()))
            .Callback<MaintenanceRecord, CancellationToken>((r, _) => captured = r)
            .Returns(Task.CompletedTask);

        var response = await _sut.CreateAsync(OwnerId, VehicleId, CreateRequest(mileage: 195_000), CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(captured);
        Assert.Equal(VehicleId, captured!.VehicleId);
        Assert.Equal(MaintenanceType.OilChange, captured.Type);
        Assert.Equal(new DateOnly(2026, 6, 1), captured.Date);
        Assert.Equal(195_000, captured.Mileage);
        Assert.Equal(120.50m, captured.Cost);
        Assert.Equal("Synthetic 5W-30", captured.Notes);
    }

    // ---------- auto-advance ----------

    [Fact]
    public async Task CreateAsync_WhenRecordMileageExceedsCurrent_AdvancesVehicleCurrentMileage()
    {
        var vehicle = VehicleWithCurrentMileage(190_000);
        OwnsVehicle(vehicle);
        _records
            .Setup(r => r.AddAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(OwnerId, VehicleId, CreateRequest(mileage: 200_000), CancellationToken.None);

        Assert.Equal(200_000, vehicle.CurrentMileage);
    }

    [Fact]
    public async Task CreateAsync_WhenRecordMileageIsBelowCurrent_LeavesCurrentMileageAlone()
    {
        var vehicle = VehicleWithCurrentMileage(190_000);
        OwnsVehicle(vehicle);
        _records
            .Setup(r => r.AddAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(OwnerId, VehicleId, CreateRequest(mileage: 185_000), CancellationToken.None);

        Assert.Equal(190_000, vehicle.CurrentMileage);
    }

    [Fact]
    public async Task UpdateAsync_WhenEditedMileageExceedsCurrent_AdvancesVehicleCurrentMileage()
    {
        var vehicle = VehicleWithCurrentMileage(190_000);
        OwnsVehicle(vehicle);
        var record = new MaintenanceRecord { Id = Guid.NewGuid(), VehicleId = VehicleId, Mileage = 188_000 };
        _records
            .Setup(r => r.FindByIdAsync(record.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _records
            .Setup(r => r.UpdateAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.UpdateAsync(OwnerId, VehicleId, record.Id, UpdateRequest(mileage: 205_000), CancellationToken.None);

        Assert.Equal(205_000, vehicle.CurrentMileage);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotPullBackCurrentMileage()
    {
        var vehicle = VehicleWithCurrentMileage(200_000);
        OwnsVehicle(vehicle);
        var record = new MaintenanceRecord { Id = Guid.NewGuid(), VehicleId = VehicleId, Mileage = 200_000 };
        _records
            .Setup(r => r.FindByIdAsync(record.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _records
            .Setup(r => r.RemoveAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var deleted = await _sut.DeleteAsync(OwnerId, VehicleId, record.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Equal(200_000, vehicle.CurrentMileage);
    }

    [Fact]
    public async Task DeleteAsync_AlsoDeletesTheFilesOfTheDocumentsItCascadesAway()
    {
        OwnsVehicle(VehicleWithCurrentMileage(200_000));
        var record = new MaintenanceRecord { Id = Guid.NewGuid(), VehicleId = VehicleId };
        _records
            .Setup(r => r.FindByIdAsync(record.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _records
            .Setup(r => r.RemoveAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _documents
            .Setup(d => d.ListByMaintenanceRecordAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Document { VehicleId = VehicleId, StorageKey = "key-a" }]);

        await _sut.DeleteAsync(OwnerId, VehicleId, record.Id, CancellationToken.None);

        // The invoice goes with the service it documents — rows by cascade, files by us.
        _storage.Verify(s => s.DeleteAsync("key-a", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRecordMissing_TouchesNoFiles()
    {
        OwnsVehicle(VehicleWithCurrentMileage(200_000));
        _records
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        await _sut.DeleteAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        _storage.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- read/update/delete happy paths ----------

    [Fact]
    public async Task ListAsync_ReturnsAResponsePerRecord()
    {
        OwnsVehicle(VehicleWithCurrentMileage(190_000));
        _records
            .Setup(r => r.ListByVehicleAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new MaintenanceRecord { VehicleId = VehicleId, Type = MaintenanceType.OilChange },
                new MaintenanceRecord { VehicleId = VehicleId, Type = MaintenanceType.TireChange },
            ]);

        var response = await _sut.ListAsync(OwnerId, VehicleId, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(2, response!.Count);
    }

    [Fact]
    public async Task GetAsync_WhenRecordMissingUnderOwnedVehicle_ReturnsNull()
    {
        OwnsVehicle(VehicleWithCurrentMileage(190_000));
        _records
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        var response = await _sut.GetAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task UpdateAsync_WhenRecordMissingUnderOwnedVehicle_ReturnsNullAndPersistsNothing()
    {
        OwnsVehicle(VehicleWithCurrentMileage(190_000));
        _records
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        var response = await _sut.UpdateAsync(OwnerId, VehicleId, Guid.NewGuid(), UpdateRequest(), CancellationToken.None);

        Assert.Null(response);
        _records.Verify(r => r.UpdateAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WritesEveryEditableField()
    {
        OwnsVehicle(VehicleWithCurrentMileage(190_000));
        var record = new MaintenanceRecord { Id = Guid.NewGuid(), VehicleId = VehicleId, Mileage = 188_000 };
        _records
            .Setup(r => r.FindByIdAsync(record.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _records
            .Setup(r => r.UpdateAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _sut.UpdateAsync(OwnerId, VehicleId, record.Id, UpdateRequest(mileage: 205_000), CancellationToken.None);

        Assert.Equal(MaintenanceType.BrakeService, response!.Type);
        Assert.Equal(new DateOnly(2026, 7, 1), response.Date);
        Assert.Equal(205_000, response.Mileage);
        Assert.Equal(300m, response.Cost);
        Assert.Equal("Front pads", response.Notes);
    }
}
