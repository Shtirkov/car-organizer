using CarOrganizer.Domain.Common;

namespace CarOrganizer.Domain.Entities;

/// <summary>
/// An uploaded file (image/PDF) stored in object storage (Cloudflare R2). Always belongs to a
/// vehicle, and may optionally be attached to a specific maintenance record.
/// </summary>
public class Document : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public Guid? MaintenanceRecordId { get; set; }
    public MaintenanceRecord? MaintenanceRecord { get; set; }

    /// <summary>Original file name as uploaded by the user.</summary>
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    /// <summary>Object key in the storage bucket (R2).</summary>
    public string StorageKey { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
}
