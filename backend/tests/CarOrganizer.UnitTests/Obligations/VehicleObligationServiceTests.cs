using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.Obligations;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Domain.Enums;
using CarOrganizer.Infrastructure.Obligations;
using Moq;

namespace CarOrganizer.UnitTests.Obligations;

/// <summary>
/// Covers <see cref="VehicleObligationService"/> against mocked stores: the vehicle-ownership gate
/// and the entity/DTO mapping.
/// </summary>
public class VehicleObligationServiceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid VehicleId = Guid.NewGuid();

    private readonly Mock<IVehicleObligationStore> _obligations = new();
    private readonly Mock<IVehicleStore> _vehicles = new();
    private readonly VehicleObligationService _sut;

    public VehicleObligationServiceTests()
    {
        _sut = new VehicleObligationService(_obligations.Object, _vehicles.Object);
    }

    private void OwnsVehicle() =>
        _vehicles
            .Setup(v => v.FindByIdAsync(VehicleId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vehicle { Id = VehicleId, OwnerId = OwnerId });

    private void OwnsNoSuchVehicle() =>
        _vehicles
            .Setup(v => v.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

    private static CreateVehicleObligationRequest CreateRequest() =>
        new(
            ObligationType.Insurance,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            450.00m,
            "Bulstrad",
            "BG/03/123456789",
            "Full year");

    private static UpdateVehicleObligationRequest UpdateRequest() =>
        new(
            ObligationType.Casco,
            new DateOnly(2026, 3, 1),
            new DateOnly(2027, 2, 28),
            1200.00m,
            "DZI",
            "CASCO-987",
            "Comprehensive");

    // ---------- ownership gate ----------

    [Fact]
    public async Task CreateAsync_WhenVehicleIsNotTheOwners_ReturnsNullAndPersistsNothing()
    {
        OwnsNoSuchVehicle();

        var response = await _sut.CreateAsync(OwnerId, VehicleId, CreateRequest(), CancellationToken.None);

        Assert.Null(response);
        _obligations.Verify(o => o.AddAsync(It.IsAny<VehicleObligation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_WhenVehicleIsNotTheOwners_ReturnsNull()
    {
        OwnsNoSuchVehicle();

        var response = await _sut.ListAsync(OwnerId, VehicleId, CancellationToken.None);

        Assert.Null(response);
        _obligations.Verify(o => o.ListByVehicleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenVehicleIsNotTheOwners_ReturnsFalse()
    {
        OwnsNoSuchVehicle();

        var deleted = await _sut.DeleteAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
        _obligations.Verify(o => o.RemoveAsync(It.IsAny<VehicleObligation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- create + mapping ----------

    [Fact]
    public async Task CreateAsync_MapsEveryFieldAndScopesToTheVehicle()
    {
        OwnsVehicle();
        VehicleObligation? captured = null;
        _obligations
            .Setup(o => o.AddAsync(It.IsAny<VehicleObligation>(), It.IsAny<CancellationToken>()))
            .Callback<VehicleObligation, CancellationToken>((o, _) => captured = o)
            .Returns(Task.CompletedTask);

        var response = await _sut.CreateAsync(OwnerId, VehicleId, CreateRequest(), CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(captured);
        Assert.Equal(VehicleId, captured!.VehicleId);
        Assert.Equal(ObligationType.Insurance, captured.Type);
        Assert.Equal(new DateOnly(2026, 1, 1), captured.ValidFrom);
        Assert.Equal(new DateOnly(2026, 12, 31), captured.ValidUntil);
        Assert.Equal(450.00m, captured.Cost);
        Assert.Equal("Bulstrad", captured.Provider);
        Assert.Equal("BG/03/123456789", captured.PolicyNumber);
        Assert.Equal("Full year", captured.Notes);
    }

    [Fact]
    public async Task CreateAsync_WithoutAValidFrom_LeavesItNull()
    {
        OwnsVehicle();
        VehicleObligation? captured = null;
        _obligations
            .Setup(o => o.AddAsync(It.IsAny<VehicleObligation>(), It.IsAny<CancellationToken>()))
            .Callback<VehicleObligation, CancellationToken>((o, _) => captured = o)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(
            OwnerId,
            VehicleId,
            new CreateVehicleObligationRequest(ObligationType.Vignette, null, new DateOnly(2026, 8, 1), 87m, null, null, null),
            CancellationToken.None);

        Assert.Null(captured!.ValidFrom);
        Assert.Equal(new DateOnly(2026, 8, 1), captured.ValidUntil);
    }

    // ---------- read / update / delete ----------

    [Fact]
    public async Task ListAsync_ReturnsAResponsePerObligation()
    {
        OwnsVehicle();
        _obligations
            .Setup(o => o.ListByVehicleAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new VehicleObligation { VehicleId = VehicleId, Type = ObligationType.Insurance },
                new VehicleObligation { VehicleId = VehicleId, Type = ObligationType.Vignette },
            ]);

        var response = await _sut.ListAsync(OwnerId, VehicleId, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(2, response!.Count);
    }

    [Fact]
    public async Task GetAsync_WhenObligationMissingUnderOwnedVehicle_ReturnsNull()
    {
        OwnsVehicle();
        _obligations
            .Setup(o => o.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleObligation?)null);

        var response = await _sut.GetAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task UpdateAsync_WritesEveryEditableField()
    {
        OwnsVehicle();
        var obligation = new VehicleObligation { Id = Guid.NewGuid(), VehicleId = VehicleId, Type = ObligationType.Insurance };
        _obligations
            .Setup(o => o.FindByIdAsync(obligation.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(obligation);
        _obligations
            .Setup(o => o.UpdateAsync(It.IsAny<VehicleObligation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _sut.UpdateAsync(OwnerId, VehicleId, obligation.Id, UpdateRequest(), CancellationToken.None);

        Assert.Equal(ObligationType.Casco, response!.Type);
        Assert.Equal(new DateOnly(2026, 3, 1), response.ValidFrom);
        Assert.Equal(new DateOnly(2027, 2, 28), response.ValidUntil);
        Assert.Equal(1200.00m, response.Cost);
        Assert.Equal("DZI", response.Provider);
        Assert.Equal("CASCO-987", response.PolicyNumber);
    }

    [Fact]
    public async Task UpdateAsync_WhenObligationMissingUnderOwnedVehicle_ReturnsNullAndPersistsNothing()
    {
        OwnsVehicle();
        _obligations
            .Setup(o => o.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleObligation?)null);

        var response = await _sut.UpdateAsync(OwnerId, VehicleId, Guid.NewGuid(), UpdateRequest(), CancellationToken.None);

        Assert.Null(response);
        _obligations.Verify(o => o.UpdateAsync(It.IsAny<VehicleObligation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenObligationExists_RemovesItAndReportsSuccess()
    {
        OwnsVehicle();
        var obligation = new VehicleObligation { Id = Guid.NewGuid(), VehicleId = VehicleId };
        _obligations
            .Setup(o => o.FindByIdAsync(obligation.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(obligation);
        _obligations
            .Setup(o => o.RemoveAsync(It.IsAny<VehicleObligation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var deleted = await _sut.DeleteAsync(OwnerId, VehicleId, obligation.Id, CancellationToken.None);

        Assert.True(deleted);
        _obligations.Verify(o => o.RemoveAsync(obligation, It.IsAny<CancellationToken>()), Times.Once);
    }
}
