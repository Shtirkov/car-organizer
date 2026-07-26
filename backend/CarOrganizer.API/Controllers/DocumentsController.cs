using CarOrganizer.API.Extensions;
using CarOrganizer.Application.Documents;
using CarOrganizer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarOrganizer.API.Controllers;

/// <summary>
/// The uploaded paperwork (invoices, policies, certificates) of one vehicle in the caller's garage.
/// Nested under the vehicle so ownership is expressed by the route: the owner comes from the token,
/// the vehicle from the URL, and anything the caller doesn't own — vehicle or document — is reported
/// as <b>404, never 403</b>.
/// </summary>
/// <remarks>
/// Documents are immutable: there is no PUT, because replacing a file is just another upload.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/vehicles/{vehicleId:guid}/documents")]
public class DocumentsController : ControllerBase
{
    /// <summary>
    /// Server-protection backstop, not the user-facing rule. The friendly 400 below owns the real
    /// limit; this only stops someone streaming something absurd, and does so as a 413.
    /// </summary>
    private const long MaxRequestBytes = DocumentLimits.MaxFileSizeBytes * 2L;

    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost]
    [RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Upload(
        Guid vehicleId,
        IFormFile? file,
        [FromForm] Guid? maintenanceRecordId,
        [FromForm] Guid? obligationId,
        CancellationToken cancellationToken)
    {
        var contentType = NormalizeContentType(file?.ContentType);

        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError("file", "A non-empty file is required.");
        }
        else if (!DocumentLimits.AllowedContentTypes.Contains(contentType))
        {
            ModelState.AddModelError(
                "file",
                $"Unsupported content type '{contentType}'. Accepted types: {DocumentLimits.AllowedContentTypesDisplay}.");
        }
        else if (file.Length > DocumentLimits.MaxFileSizeBytes)
        {
            ModelState.AddModelError(
                "file",
                $"The file is larger than the {DocumentLimits.MaxFileSizeBytes / (1024 * 1024)} MB limit.");
        }

        if (maintenanceRecordId is null && obligationId is null)
        {
            ModelState.AddModelError(
                nameof(maintenanceRecordId),
                "A document must be attached to a maintenance record or an obligation. "
                + "Supply exactly one of maintenanceRecordId or obligationId.");
        }
        else if (maintenanceRecordId is not null && obligationId is not null)
        {
            ModelState.AddModelError(
                nameof(obligationId),
                "A document attaches to either a maintenance record or an obligation, not both.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await using var content = file!.OpenReadStream();

        var request = new UploadDocumentRequest(
            content, file.FileName, contentType, file.Length, maintenanceRecordId, obligationId);

        var document = await _documentService.UploadAsync(User.GetUserId(), vehicleId, request, cancellationToken);

        return document is null
            ? NotFound()
            : CreatedAtAction(nameof(Get), new { vehicleId, id = document.Id }, document);
    }

    [HttpGet]
    public async Task<IActionResult> List(Guid vehicleId, CancellationToken cancellationToken)
    {
        var documents = await _documentService.ListAsync(User.GetUserId(), vehicleId, cancellationToken);

        // null distinguishes "not your vehicle" (404) from an owned vehicle with no documents (200, []).
        return documents is null ? NotFound() : Ok(documents);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetAsync(User.GetUserId(), vehicleId, id, cancellationToken);

        return document is null ? NotFound() : Ok(document);
    }

    /// <summary>Streams the stored bytes back. Separate from <see cref="Get"/>, which is metadata only.</summary>
    [HttpGet("{id:guid}/content")]
    public async Task<IActionResult> Download(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        var download = await _documentService.DownloadAsync(User.GetUserId(), vehicleId, id, cancellationToken);

        return download is null
            ? NotFound()
            : File(download.Content, download.ContentType, download.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _documentService.DeleteAsync(User.GetUserId(), vehicleId, id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Drops any parameters a client tacked onto the media type (<c>image/jpeg; charset=...</c>) so the
    /// value compared against the allowlist — and stored — is the media type alone.
    /// </summary>
    private static string NormalizeContentType(string? contentType) =>
        contentType?.Split(';')[0].Trim() ?? string.Empty;
}
