using CarOrganizer.Domain.Common;

namespace CarOrganizer.Domain.Entities;

/// <summary>
/// An uploaded file (image/PDF) held by the configured file storage. Always belongs to a vehicle,
/// and may optionally be attached to either a specific maintenance record (the invoice for a
/// service) or a specific obligation (the policy or certificate behind it) — never both.
/// </summary>
public class Document : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public Guid? MaintenanceRecordId { get; set; }
    public MaintenanceRecord? MaintenanceRecord { get; set; }

    public Guid? VehicleObligationId { get; set; }
    public VehicleObligation? VehicleObligation { get; set; }

    /// <summary>Original file name as uploaded by the user.</summary>
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    /// <summary>Opaque key the file storage identifies the stored bytes by.</summary>
    public string StorageKey { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
}
