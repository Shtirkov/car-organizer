using CarOrganizer.Domain.Common;

namespace CarOrganizer.Domain.Entities;

/// <summary>
/// A long-lived token used to obtain fresh access tokens. Unlike the stateless JWT access token,
/// refresh tokens are persisted (as a hash) so they can be rotated and revoked server-side.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>SHA-256 hash of the raw token. The raw value is only ever held by the client.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>Set when the token is rotated or explicitly revoked; a revoked token can no longer be used.</summary>
    public DateTime? RevokedAtUtc { get; set; }

    /// <summary>True while the token is neither revoked nor past its expiry.</summary>
    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
