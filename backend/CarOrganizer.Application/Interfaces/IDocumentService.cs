using CarOrganizer.Application.Documents;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Document operations for a single vehicle in a single owner's garage. Implemented in the
/// Infrastructure layer.
/// </summary>
/// <remarks>
/// Every method first proves the vehicle is the caller's, so a <c>null</c> (or <c>false</c>) return
/// covers "no such vehicle / not yours" and "no such document" alike — indistinguishable by design
/// (the 404-not-403 rule). Plain values rather than <see cref="Common.Result"/>: the shape errors an
/// upload can carry (content type, size, both links set) are rejected by the controller before the
/// service runs.
/// </remarks>
public interface IDocumentService
{
    /// <summary>
    /// The stored document, or <c>null</c> if the vehicle isn't the owner's, or the maintenance record
    /// / obligation being linked to isn't on that vehicle. Every document must name exactly one of the
    /// two; a request naming neither is refused (and the controller turns that away as a 400 first).
    /// </summary>
    Task<DocumentResponse?> UploadAsync(Guid ownerId, Guid vehicleId, UploadDocumentRequest request, CancellationToken cancellationToken = default);

    /// <summary>The vehicle's documents, or <c>null</c> if the vehicle doesn't exist or isn't the owner's.</summary>
    Task<IReadOnlyList<DocumentResponse>?> ListAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>The document's metadata, or <c>null</c> if the vehicle/document doesn't exist or isn't the owner's.</summary>
    Task<DocumentResponse?> GetAsync(Guid ownerId, Guid vehicleId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The document's bytes, or <c>null</c> if the vehicle/document doesn't exist, isn't the owner's,
    /// or the stored file has gone missing underneath the metadata row.
    /// </summary>
    Task<DocumentDownload?> DownloadAsync(Guid ownerId, Guid vehicleId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary><c>true</c> if a document was deleted; <c>false</c> if the vehicle/document wasn't found or isn't the owner's.</summary>
    Task<bool> DeleteAsync(Guid ownerId, Guid vehicleId, Guid documentId, CancellationToken cancellationToken = default);
}
