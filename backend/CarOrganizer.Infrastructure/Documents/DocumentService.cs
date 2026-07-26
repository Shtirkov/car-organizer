using CarOrganizer.Application.Documents;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Infrastructure.Documents;

/// <summary>
/// Store-backed implementation of <see cref="IDocumentService"/>. Threads the owner id through an
/// ownership check on the vehicle before touching its documents, keeps the stored bytes and the
/// metadata row in step, and owns the entity/DTO mapping.
/// </summary>
public class DocumentService : IDocumentService
{
    private const string FallbackFileName = "document";

    private readonly IDocumentStore _documents;
    private readonly IVehicleStore _vehicles;
    private readonly IMaintenanceRecordStore _records;
    private readonly IVehicleObligationStore _obligations;
    private readonly IFileStorage _storage;

    public DocumentService(
        IDocumentStore documents,
        IVehicleStore vehicles,
        IMaintenanceRecordStore records,
        IVehicleObligationStore obligations,
        IFileStorage storage)
    {
        _documents = documents;
        _vehicles = vehicles;
        _records = records;
        _obligations = obligations;
        _storage = storage;
    }

    public async Task<DocumentResponse?> UploadAsync(Guid ownerId, Guid vehicleId, UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (!await OwnsVehicleAsync(ownerId, vehicleId, cancellationToken))
        {
            return null;
        }

        // Checked before a single byte is written, so a bad link can't leave an orphaned blob behind.
        if (!await LinkTargetExistsAsync(vehicleId, request, cancellationToken))
        {
            return null;
        }

        var storageKey = await _storage.SaveAsync(request.Content, cancellationToken);

        var document = new Document
        {
            VehicleId = vehicleId,
            MaintenanceRecordId = request.MaintenanceRecordId,
            VehicleObligationId = request.ObligationId,
            FileName = SanitizeFileName(request.FileName),
            ContentType = request.ContentType,
            StorageKey = storageKey,
            SizeBytes = request.SizeBytes,
        };

        try
        {
            await _documents.AddAsync(document, cancellationToken);
        }
        catch
        {
            // The bytes landed but the row didn't, so nothing will ever point at them again. Clean up
            // on an uncancellable token — a cancelled request is exactly when this needs to still run.
            await _storage.DeleteAsync(storageKey, CancellationToken.None);
            throw;
        }

        return ToResponse(document);
    }

    public async Task<IReadOnlyList<DocumentResponse>?> ListAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        if (!await OwnsVehicleAsync(ownerId, vehicleId, cancellationToken))
        {
            return null;
        }

        var documents = await _documents.ListByVehicleAsync(vehicleId, cancellationToken);

        return documents.Select(ToResponse).ToArray();
    }

    public async Task<DocumentResponse?> GetAsync(Guid ownerId, Guid vehicleId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await FindOwnedAsync(ownerId, vehicleId, documentId, cancellationToken);

        return document is null ? null : ToResponse(document);
    }

    public async Task<DocumentDownload?> DownloadAsync(Guid ownerId, Guid vehicleId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await FindOwnedAsync(ownerId, vehicleId, documentId, cancellationToken);
        if (document is null)
        {
            return null;
        }

        var content = await _storage.OpenReadAsync(document.StorageKey, cancellationToken);
        if (content is null)
        {
            // Metadata outlived its bytes. Nothing the caller can do about it, and nothing to hand
            // back — a 404 is more honest than a stream that isn't there.
            return null;
        }

        return new DocumentDownload(content, document.ContentType, document.FileName);
    }

    public async Task<bool> DeleteAsync(Guid ownerId, Guid vehicleId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await FindOwnedAsync(ownerId, vehicleId, documentId, cancellationToken);
        if (document is null)
        {
            return false;
        }

        // Row first, then bytes. A blob that outlives its row is invisible and reclaimable later,
        // whereas bytes deleted ahead of a failed row removal would leave a document that exists in
        // every listing but 404s the moment anyone downloads it.
        await _documents.RemoveAsync(document, cancellationToken);
        await _storage.DeleteAsync(document.StorageKey, cancellationToken);

        return true;
    }

    private async Task<Document?> FindOwnedAsync(Guid ownerId, Guid vehicleId, Guid documentId, CancellationToken cancellationToken)
    {
        if (!await OwnsVehicleAsync(ownerId, vehicleId, cancellationToken))
        {
            return null;
        }

        return await _documents.FindByIdAsync(documentId, vehicleId, cancellationToken);
    }

    private async Task<bool> OwnsVehicleAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken) =>
        await _vehicles.FindByIdAsync(vehicleId, ownerId, cancellationToken) is not null;

    /// <summary>
    /// Whether the record or obligation the upload hangs off actually belongs to this vehicle. Every
    /// document must name exactly one of them — a file we can't say the purpose of is worse than no
    /// file — so a request naming neither is refused here too, not just by the controller.
    /// </summary>
    private async Task<bool> LinkTargetExistsAsync(Guid vehicleId, UploadDocumentRequest request, CancellationToken cancellationToken)
    {
        if (request.MaintenanceRecordId is Guid recordId)
        {
            return await _records.FindByIdAsync(recordId, vehicleId, cancellationToken) is not null;
        }

        if (request.ObligationId is Guid obligationId)
        {
            return await _obligations.FindByIdAsync(obligationId, vehicleId, cancellationToken) is not null;
        }

        // Nothing named. Unreachable over HTTP (the controller answers that with a 400 that explains
        // itself), kept as a refusal so no other caller can create paperwork of unknown purpose.
        return false;
    }

    /// <summary>
    /// Reduces a client-supplied file name to something safe to store and echo back. It never reaches
    /// the filesystem — <see cref="IFileStorage"/> generates its own keys — but it is echoed in a
    /// download's Content-Disposition, so directory parts are stripped and the length is bounded.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        // Both separators, because the server's own path convention says nothing about the client's.
        var leaf = fileName.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim();

        if (string.IsNullOrEmpty(leaf))
        {
            return FallbackFileName;
        }

        // Truncation can cost the extension, which is harmless: ContentType is what drives rendering.
        return leaf.Length > DocumentLimits.FileNameMaxLength
            ? leaf[..DocumentLimits.FileNameMaxLength]
            : leaf;
    }

    private static DocumentResponse ToResponse(Document document) =>
        new(
            document.Id,
            document.FileName,
            document.ContentType,
            document.SizeBytes,
            document.MaintenanceRecordId,
            document.VehicleObligationId,
            document.CreatedAtUtc,
            document.UpdatedAtUtc);
}
