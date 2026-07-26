using CarOrganizer.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace CarOrganizer.Infrastructure.Storage;

/// <summary>
/// <see cref="IFileStorage"/> over a directory on the local filesystem — what runs in development and
/// in tests, until Phase 8 swaps in an S3-compatible bucket behind the same interface.
/// </summary>
/// <remarks>
/// Keys are server-generated GUIDs, never anything derived from the uploaded file name, so no request
/// can steer a write or a read outside the root directory. Uploads therefore land as extension-less
/// files: the original name and content type live in the database, not on disk.
/// </remarks>
public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IOptions<FileStorageSettings> settings)
    {
        _root = Path.GetFullPath(settings.Value.LocalRoot);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, CancellationToken cancellationToken = default)
    {
        var storageKey = Guid.NewGuid().ToString("N");

        await using var file = File.Create(PathFor(storageKey));
        await content.CopyToAsync(file, cancellationToken);

        return storageKey;
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = PathFor(storageKey);

        // A missing file means the metadata row outlived its bytes; the service reports that as a 404
        // rather than letting an IOException surface as a 500.
        Stream? content = File.Exists(path) ? File.OpenRead(path) : null;

        return Task.FromResult(content);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        // File.Delete is already a no-op for a missing path, which gives us idempotency for free.
        File.Delete(PathFor(storageKey));

        return Task.CompletedTask;
    }

    private string PathFor(string storageKey) => Path.Combine(_root, storageKey);
}
