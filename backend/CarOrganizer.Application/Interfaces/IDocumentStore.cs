using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Persistence gateway for document metadata. Implemented in the Infrastructure layer.
/// </summary>
/// <remarks>
/// Lookups are scoped by <c>vehicleId</c>, as with the other nested resources: a document only makes
/// sense under its vehicle, and the caller has already been proven to own that vehicle, so a document
/// under a different vehicle simply isn't found. There is no <c>UpdateAsync</c> — an uploaded file is
/// immutable, and replacing one is a new upload.
/// </remarks>
public interface IDocumentStore
{
    Task AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>Every document for the vehicle, most recently uploaded first.</summary>
    Task<IReadOnlyList<Document>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>The vehicle's document with this id, or <c>null</c> if there is no such document.</summary>
    Task<Document?> FindByIdAsync(Guid documentId, Guid vehicleId, CancellationToken cancellationToken = default);

    Task RemoveAsync(Document document, CancellationToken cancellationToken = default);
}
