namespace CarOrganizer.Domain.Common;

/// <summary>
/// Base type for all persisted domain entities. Provides an identity and audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
