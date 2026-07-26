using CarOrganizer.Domain.Common;

namespace CarOrganizer.Domain.Entities;

/// <summary>
/// An uploaded file (image/PDF) held by the configured file storage. Always belongs to a vehicle,
/// and is attached to <b>exactly one</b> of a maintenance record (the invoice for a service) or an
/// obligation (the policy or certificate behind it) — never both, and never neither: paperwork whose
/// purpose nobody can name is worse than no paperwork at all.
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
