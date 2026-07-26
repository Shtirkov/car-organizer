using CarOrganizer.Application.Documents;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Infrastructure.Documents;
using Moq;

namespace CarOrganizer.UnitTests.Documents;

/// <summary>
/// Covers <see cref="DocumentService"/> against mocked stores and storage: the vehicle-ownership gate,
/// the link-target check, file-name sanitisation, and keeping the stored bytes and the metadata row
/// in step in both directions.
/// </summary>
public class DocumentServiceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid VehicleId = Guid.NewGuid();
    private static readonly Guid RecordId = Guid.NewGuid();
    private const string StorageKey = "3f2504e04f8911d39a0c0305e82c3301";

    private readonly Mock<IDocumentStore> _documents = new();
    private readonly Mock<IVehicleStore> _vehicles = new();
    private readonly Mock<IMaintenanceRecordStore> _records = new();
    private readonly Mock<IVehicleObligationStore> _obligations = new();
    private readonly Mock<IFileStorage> _storage = new();
    private readonly DocumentService _sut;

    public DocumentServiceTests()
    {
        _storage
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(StorageKey);

        // Every document must hang off a record or an obligation, so the default request links to one.
        _records
            .Setup(r => r.FindByIdAsync(RecordId, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MaintenanceRecord { Id = RecordId, VehicleId = VehicleId });

        _sut = new DocumentService(
            _documents.Object, _vehicles.Object, _records.Object, _obligations.Object, _storage.Object);
    }

    private void OwnsVehicle() =>
        _vehicles
            .Setup(v => v.FindByIdAsync(VehicleId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vehicle { Id = VehicleId, OwnerId = OwnerId });

    private void OwnsNoSuchVehicle() =>
        _vehicles
            .Setup(v => v.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

    /// <summary>The ordinary case: an upload linked to a maintenance record that is on the vehicle.</summary>
    private static UploadDocumentRequest UploadRequest(
        string fileName = "invoice.pdf",
        string contentType = "application/pdf") =>
        new(new MemoryStream([1, 2, 3]), fileName, contentType, 3, RecordId, null);

    private static UploadDocumentRequest LinkedRequest(Guid? maintenanceRecordId, Guid? obligationId) =>
        new(new MemoryStream([1, 2, 3]), "invoice.pdf", "application/pdf", 3, maintenanceRecordId, obligationId);

    private static Document StoredDocument(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            VehicleId = VehicleId,
            FileName = "invoice.pdf",
            ContentType = "application/pdf",
            StorageKey = StorageKey,
            SizeBytes = 3,
        };

    // ---------- ownership gate ----------

    [Fact]
    public async Task UploadAsync_WhenVehicleIsNotTheOwners_ReturnsNullAndWritesNothing()
    {
        OwnsNoSuchVehicle();

        var response = await _sut.UploadAsync(OwnerId, VehicleId, UploadRequest(), CancellationToken.None);

        Assert.Null(response);
        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
        _documents.Verify(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_WhenVehicleIsNotTheOwners_ReturnsNull()
    {
        OwnsNoSuchVehicle();

        var response = await _sut.ListAsync(OwnerId, VehicleId, CancellationToken.None);

        Assert.Null(response);
        _documents.Verify(d => d.ListByVehicleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_WhenVehicleIsNotTheOwners_ReturnsNull()
    {
        OwnsNoSuchVehicle();

        var response = await _sut.GetAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
        _documents.Verify(d => d.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DownloadAsync_WhenVehicleIsNotTheOwners_ReturnsNullAndOpensNothing()
    {
        OwnsNoSuchVehicle();

        var download = await _sut.DownloadAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(download);
        _storage.Verify(s => s.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenVehicleIsNotTheOwners_ReturnsFalseAndDeletesNothing()
    {
        OwnsNoSuchVehicle();

        var deleted = await _sut.DeleteAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
        _documents.Verify(d => d.RemoveAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _storage.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- upload: mapping ----------

    [Fact]
    public async Task UploadAsync_MapsEveryFieldAndScopesToTheVehicle()
    {
        OwnsVehicle();
        Document? captured = null;
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => captured = d)
            .Returns(Task.CompletedTask);

        var response = await _sut.UploadAsync(
            OwnerId, VehicleId, UploadRequest("receipt.jpg", "image/jpeg"), CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(captured);
        Assert.Equal(VehicleId, captured!.VehicleId);
        Assert.Equal("receipt.jpg", captured.FileName);
        Assert.Equal("image/jpeg", captured.ContentType);
        Assert.Equal(StorageKey, captured.StorageKey);
        Assert.Equal(3, captured.SizeBytes);
        Assert.Equal(RecordId, captured.MaintenanceRecordId);
        Assert.Null(captured.VehicleObligationId);
    }

    [Fact]
    public async Task UploadAsync_ReturnsTheStoredMetadataWithoutTheStorageKey()
    {
        OwnsVehicle();
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _sut.UploadAsync(
            OwnerId, VehicleId, UploadRequest("receipt.jpg", "image/jpeg"), CancellationToken.None);

        Assert.Equal("receipt.jpg", response!.FileName);
        Assert.Equal("image/jpeg", response.ContentType);
        Assert.Equal(3, response.SizeBytes);
        Assert.NotEqual(Guid.Empty, response.Id);
    }

    [Fact]
    public async Task UploadAsync_SavesTheBytesBeforeTheRow()
    {
        OwnsVehicle();
        var sequence = new List<string>();
        _storage
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("blob"))
            .ReturnsAsync(StorageKey);
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("row"))
            .Returns(Task.CompletedTask);

        await _sut.UploadAsync(OwnerId, VehicleId, UploadRequest(), CancellationToken.None);

        // A row is only ever written once its bytes exist, so no listing can name a missing file.
        Assert.Equal(["blob", "row"], sequence);
    }

    // ---------- upload: link targets ----------

    [Fact]
    public async Task UploadAsync_LinkedToAMaintenanceRecordOnThisVehicle_PersistsTheLink()
    {
        OwnsVehicle();
        var recordId = Guid.NewGuid();
        _records
            .Setup(r => r.FindByIdAsync(recordId, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MaintenanceRecord { Id = recordId, VehicleId = VehicleId });
        Document? captured = null;
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => captured = d)
            .Returns(Task.CompletedTask);

        var response = await _sut.UploadAsync(
            OwnerId, VehicleId, LinkedRequest(recordId, null), CancellationToken.None);

        Assert.Equal(recordId, captured!.MaintenanceRecordId);
        Assert.Equal(recordId, response!.MaintenanceRecordId);
        Assert.Null(captured.VehicleObligationId);
    }

    [Fact]
    public async Task UploadAsync_LinkedToAnObligationOnThisVehicle_PersistsTheLink()
    {
        OwnsVehicle();
        var obligationId = Guid.NewGuid();
        _obligations
            .Setup(o => o.FindByIdAsync(obligationId, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehicleObligation { Id = obligationId, VehicleId = VehicleId });
        Document? captured = null;
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => captured = d)
            .Returns(Task.CompletedTask);

        var response = await _sut.UploadAsync(
            OwnerId, VehicleId, LinkedRequest(null, obligationId), CancellationToken.None);

        Assert.Equal(obligationId, captured!.VehicleObligationId);
        Assert.Equal(obligationId, response!.VehicleObligationId);
        Assert.Null(captured.MaintenanceRecordId);
    }

    [Fact]
    public async Task UploadAsync_WhenTheMaintenanceRecordIsNotOnThisVehicle_ReturnsNullAndWritesNoBytes()
    {
        OwnsVehicle();
        _records
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        var response = await _sut.UploadAsync(
            OwnerId, VehicleId, LinkedRequest(Guid.NewGuid(), null), CancellationToken.None);

        Assert.Null(response);
        // The link is checked first precisely so a rejected upload leaves no orphaned blob behind.
        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
        _documents.Verify(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_WhenTheObligationIsNotOnThisVehicle_ReturnsNullAndWritesNoBytes()
    {
        OwnsVehicle();
        _obligations
            .Setup(o => o.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleObligation?)null);

        var response = await _sut.UploadAsync(
            OwnerId, VehicleId, LinkedRequest(null, Guid.NewGuid()), CancellationToken.None);

        Assert.Null(response);
        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_WithoutALink_ReturnsNullAndWritesNothing()
    {
        OwnsVehicle();

        var response = await _sut.UploadAsync(
            OwnerId, VehicleId, LinkedRequest(null, null), CancellationToken.None);

        // A file nobody can say the purpose of is worse than no file: the controller turns this away
        // as a 400, and the service refuses it too so no other caller can slip one in.
        Assert.Null(response);
        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
        _documents.Verify(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_LinkedToAnObligation_DoesNotConsultTheRecordStore()
    {
        OwnsVehicle();
        var obligationId = Guid.NewGuid();
        _obligations
            .Setup(o => o.FindByIdAsync(obligationId, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehicleObligation { Id = obligationId, VehicleId = VehicleId });
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.UploadAsync(OwnerId, VehicleId, LinkedRequest(null, obligationId), CancellationToken.None);

        _records.Verify(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- upload: compensation ----------

    [Fact]
    public async Task UploadAsync_WhenTheRowFails_DeletesTheOrphanedBlobAndRethrows()
    {
        OwnsVehicle();
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("the database went away"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UploadAsync(OwnerId, VehicleId, UploadRequest(), CancellationToken.None));

        // Nothing would ever point at those bytes again, so they must not survive the failure.
        _storage.Verify(s => s.DeleteAsync(StorageKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_WhenTheRowFailsOnACancelledRequest_StillCleansUp()
    {
        OwnsVehicle();
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.UploadAsync(OwnerId, VehicleId, UploadRequest(), cancelled.Token));

        // The compensation runs on CancellationToken.None — a cancelled request is exactly the case
        // where passing the caller's token through would skip the cleanup that is needed most.
        _storage.Verify(s => s.DeleteAsync(StorageKey, CancellationToken.None), Times.Once);
    }

    // ---------- upload: file name sanitisation ----------

    [Theory]
    [InlineData("../../etc/passwd", "passwd")]
    [InlineData(@"C:\Users\alice\Desktop\scan.pdf", "scan.pdf")]
    [InlineData("/var/tmp/photo.jpg", "photo.jpg")]
    [InlineData("  spaced.pdf  ", "spaced.pdf")]
    public async Task UploadAsync_ReducesTheFileNameToItsLeaf(string uploaded, string expected)
    {
        OwnsVehicle();
        Document? captured = null;
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => captured = d)
            .Returns(Task.CompletedTask);

        await _sut.UploadAsync(OwnerId, VehicleId, UploadRequest(uploaded), CancellationToken.None);

        Assert.Equal(expected, captured!.FileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/")]
    [InlineData(@"\\")]
    public async Task UploadAsync_WithAnUnusableFileName_FallsBackToADefault(string uploaded)
    {
        OwnsVehicle();
        Document? captured = null;
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => captured = d)
            .Returns(Task.CompletedTask);

        await _sut.UploadAsync(OwnerId, VehicleId, UploadRequest(uploaded), CancellationToken.None);

        Assert.Equal("document", captured!.FileName);
    }

    [Fact]
    public async Task UploadAsync_TruncatesAFileNameLongerThanTheColumn()
    {
        OwnsVehicle();
        Document? captured = null;
        _documents
            .Setup(d => d.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => captured = d)
            .Returns(Task.CompletedTask);

        await _sut.UploadAsync(
            OwnerId, VehicleId, UploadRequest(new string('a', 400) + ".pdf"), CancellationToken.None);

        // Longer than the FileName column would accept, so it is cut rather than left to fail the insert.
        Assert.Equal(DocumentLimits.FileNameMaxLength, captured!.FileName.Length);
    }

    // ---------- read ----------

    [Fact]
    public async Task ListAsync_ReturnsAResponsePerDocument()
    {
        OwnsVehicle();
        _documents
            .Setup(d => d.ListByVehicleAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([StoredDocument(), StoredDocument()]);

        var response = await _sut.ListAsync(OwnerId, VehicleId, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(2, response!.Count);
    }

    [Fact]
    public async Task ListAsync_WithNoDocuments_ReturnsAnEmptyListNotNull()
    {
        OwnsVehicle();
        _documents
            .Setup(d => d.ListByVehicleAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _sut.ListAsync(OwnerId, VehicleId, CancellationToken.None);

        // null means "not your vehicle"; an owned vehicle with no paperwork is an empty list.
        Assert.NotNull(response);
        Assert.Empty(response!);
    }

    [Fact]
    public async Task GetAsync_WhenDocumentMissingUnderOwnedVehicle_ReturnsNull()
    {
        OwnsVehicle();
        _documents
            .Setup(d => d.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var response = await _sut.GetAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetAsync_LooksTheDocumentUpUnderItsVehicle()
    {
        OwnsVehicle();
        var document = StoredDocument();
        _documents
            .Setup(d => d.FindByIdAsync(document.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var response = await _sut.GetAsync(OwnerId, VehicleId, document.Id, CancellationToken.None);

        Assert.Equal(document.Id, response!.Id);
        _documents.Verify(d => d.FindByIdAsync(document.Id, VehicleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- download ----------

    [Fact]
    public async Task DownloadAsync_ReturnsTheBytesWithTheStoredMetadata()
    {
        OwnsVehicle();
        var document = StoredDocument();
        _documents
            .Setup(d => d.FindByIdAsync(document.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        var content = new MemoryStream([9, 8, 7]);
        _storage
            .Setup(s => s.OpenReadAsync(StorageKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        var download = await _sut.DownloadAsync(OwnerId, VehicleId, document.Id, CancellationToken.None);

        Assert.NotNull(download);
        Assert.Same(content, download!.Content);
        Assert.Equal("application/pdf", download.ContentType);
        Assert.Equal("invoice.pdf", download.FileName);
    }

    [Fact]
    public async Task DownloadAsync_WhenTheBlobIsGoneUnderTheRow_ReturnsNull()
    {
        OwnsVehicle();
        var document = StoredDocument();
        _documents
            .Setup(d => d.FindByIdAsync(document.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _storage
            .Setup(s => s.OpenReadAsync(StorageKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        var download = await _sut.DownloadAsync(OwnerId, VehicleId, document.Id, CancellationToken.None);

        Assert.Null(download);
    }

    // ---------- delete ----------

    [Fact]
    public async Task DeleteAsync_RemovesTheRowThenTheBytes()
    {
        OwnsVehicle();
        var document = StoredDocument();
        _documents
            .Setup(d => d.FindByIdAsync(document.Id, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        var sequence = new List<string>();
        _documents
            .Setup(d => d.RemoveAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("row"))
            .Returns(Task.CompletedTask);
        _storage
            .Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("blob"))
            .Returns(Task.CompletedTask);

        var deleted = await _sut.DeleteAsync(OwnerId, VehicleId, document.Id, CancellationToken.None);

        Assert.True(deleted);
        // Deleting the bytes first would leave a document that lists fine but 404s on download.
        Assert.Equal(["row", "blob"], sequence);
        _storage.Verify(s => s.DeleteAsync(StorageKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenDocumentMissingUnderOwnedVehicle_ReturnsFalseAndDeletesNothing()
    {
        OwnsVehicle();
        _documents
            .Setup(d => d.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var deleted = await _sut.DeleteAsync(OwnerId, VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
        _documents.Verify(d => d.RemoveAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _storage.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
