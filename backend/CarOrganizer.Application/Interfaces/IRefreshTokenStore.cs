using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Application.Interfaces;

/// <summary>Persistence for refresh tokens. Implemented in the Infrastructure layer.</summary>
public interface IRefreshTokenStore
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>Looks up a refresh token by its hash, including the owning <see cref="User"/>.</summary>
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an already-tracked token (e.g. after revoking it).</summary>
    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
