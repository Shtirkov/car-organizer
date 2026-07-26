namespace CarOrganizer.Application.Documents;

/// <summary>
/// One file on its way into storage, already unpacked from its multipart envelope by the controller.
/// </summary>
/// <remarks>
/// Deliberately carries a plain <see cref="Stream"/> rather than an <c>IFormFile</c>: the Application
/// layer takes no ASP.NET dependency, and the service can be exercised with a <c>MemoryStream</c>.
/// Exactly one of <see cref="MaintenanceRecordId"/> and <see cref="ObligationId"/> must be set — the
/// controller rejects both-or-neither with a 400 before this record is built.
/// </remarks>
public record UploadDocumentRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes,
    Guid? MaintenanceRecordId,
    Guid? ObligationId);
