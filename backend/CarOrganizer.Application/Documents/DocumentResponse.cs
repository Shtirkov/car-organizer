namespace CarOrganizer.Application.Documents;

/// <summary>
/// A document's metadata as returned to the owner. Carries no <c>VehicleId</c> — the vehicle is
/// already fixed by the URL the document was reached through — and no storage key, which is an
/// internal detail of whichever <c>IFileStorage</c> happens to be configured.
/// </summary>
public record DocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    Guid? MaintenanceRecordId,
    Guid? VehicleObligationId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
