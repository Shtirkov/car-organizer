namespace CarOrganizer.Application.Interfaces;

/// <summary>
/// Where uploaded bytes actually live. Implemented in the Infrastructure layer — on local disk while
/// developing, and behind this same contract by an S3-compatible bucket (Cloudflare R2) once deployed.
/// </summary>
/// <remarks>
/// The storage owns its key scheme: <see cref="SaveAsync"/> hands back the key it chose rather than
/// accepting one, so no caller-supplied string ever reaches a file path or object name.
/// </remarks>
public interface IFileStorage
{
    /// <summary>Stores the stream's contents and returns the key they can be read back by.</summary>
    Task<string> SaveAsync(Stream content, CancellationToken cancellationToken = default);

    /// <summary>The stored bytes, or <c>null</c> if nothing is stored under this key.</summary>
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>Removes the stored bytes. Idempotent — deleting an absent key is not an error.</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
