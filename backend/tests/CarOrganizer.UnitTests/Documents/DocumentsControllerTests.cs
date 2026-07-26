using System.Security.Claims;
using CarOrganizer.API.Controllers;
using CarOrganizer.Application.Documents;
using CarOrganizer.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;

namespace CarOrganizer.UnitTests.Documents;

/// <summary>
/// Covers <see cref="DocumentsController"/>: the shape validation an upload has to clear before the
/// service is consulted (file present, allowed type, size, and <b>exactly one</b> link), and mapping
/// the service's outcome to the right HTTP result.
/// </summary>
public class DocumentsControllerTests
{
    private static readonly Guid CallerId = Guid.NewGuid();
    private static readonly Guid VehicleId = Guid.NewGuid();
    private static readonly Guid RecordId = Guid.NewGuid();

    private readonly Mock<IDocumentService> _service = new();
    private readonly DocumentsController _sut;

    public DocumentsControllerTests()
    {
        _sut = new DocumentsController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", CallerId.ToString())], "Test")),
                },
            },
        };

        // ValidationProblem() reaches for this through DI, which no bare controller instance has.
        var problemDetails = new Mock<ProblemDetailsFactory>();
        problemDetails
            .Setup(f => f.CreateValidationProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<ModelStateDictionary>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new ValidationProblemDetails { Status = StatusCodes.Status400BadRequest });
        _sut.ProblemDetailsFactory = problemDetails.Object;
    }

    /// <summary>
    /// A form file whose declared length is independent of the bytes behind it, so the size rule can
    /// be exercised without materialising 15 MB.
    /// </summary>
    private static FormFile FileOf(string contentType = "application/pdf", long length = 3, string fileName = "invoice.pdf")
    {
        var stream = new MemoryStream([1, 2, 3]);

        return new FormFile(stream, 0, length, "file", fileName)
        {
            Headers = new HeaderDictionary { ["Content-Type"] = contentType },
        };
    }

    private static DocumentResponse SampleResponse(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), "invoice.pdf", "application/pdf", 3, null, null, DateTime.UtcNow, null);

    private void ServiceStores(DocumentResponse? response) =>
        _service
            .Setup(s => s.UploadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UploadDocumentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    private void VerifyServiceNotCalled() =>
        _service.Verify(
            s => s.UploadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UploadDocumentRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);

    // ---------- upload validation ----------

    [Fact]
    public async Task Upload_WithNoFile_ReturnsBadRequestAndDoesNotCallTheService()
    {
        var response = await _sut.Upload(VehicleId, null, RecordId, null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response);
        VerifyServiceNotCalled();
    }

    [Fact]
    public async Task Upload_WithAnEmptyFile_ReturnsBadRequest()
    {
        var response = await _sut.Upload(VehicleId, FileOf(length: 0), RecordId, null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response);
        VerifyServiceNotCalled();
    }

    [Theory]
    [InlineData("image/heic")]
    [InlineData("image/heif")]
    [InlineData("text/plain")]
    [InlineData("application/zip")]
    [InlineData("")]
    public async Task Upload_WithAnUnsupportedContentType_ReturnsBadRequest(string contentType)
    {
        var response = await _sut.Upload(VehicleId, FileOf(contentType), RecordId, null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response);
        VerifyServiceNotCalled();
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("application/pdf")]
    [InlineData("IMAGE/JPEG")]
    public async Task Upload_WithAnAllowedContentType_ReachesTheService(string contentType)
    {
        ServiceStores(SampleResponse());

        var response = await _sut.Upload(VehicleId, FileOf(contentType), RecordId, null, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(response);
    }

    [Fact]
    public async Task Upload_WithAContentTypeCarryingParameters_StripsThemBeforeStoring()
    {
        UploadDocumentRequest? captured = null;
        _service
            .Setup(s => s.UploadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UploadDocumentRequest>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, UploadDocumentRequest, CancellationToken>((_, _, r, _) => captured = r)
            .ReturnsAsync(SampleResponse());

        var response = await _sut.Upload(
            VehicleId, FileOf("image/jpeg; charset=binary"), RecordId, null, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal("image/jpeg", captured!.ContentType);
    }

    [Fact]
    public async Task Upload_LargerThanTheLimit_ReturnsBadRequest()
    {
        var response = await _sut.Upload(
            VehicleId, FileOf(length: DocumentLimits.MaxFileSizeBytes + 1), RecordId, null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response);
        VerifyServiceNotCalled();
    }

    [Fact]
    public async Task Upload_ExactlyAtTheLimit_IsAccepted()
    {
        ServiceStores(SampleResponse());

        var response = await _sut.Upload(
            VehicleId, FileOf(length: DocumentLimits.MaxFileSizeBytes), RecordId, null, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(response);
    }

    [Fact]
    public async Task Upload_WithNoLinkIds_ReturnsBadRequest()
    {
        var response = await _sut.Upload(VehicleId, FileOf(), null, null, CancellationToken.None);

        // A document has to say what it is paperwork for; a vehicle alone isn't enough.
        Assert.IsType<BadRequestObjectResult>(response);
        VerifyServiceNotCalled();
    }

    [Fact]
    public async Task Upload_WithBothLinkIds_ReturnsBadRequest()
    {
        var response = await _sut.Upload(
            VehicleId, FileOf(), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response);
        VerifyServiceNotCalled();
    }

    [Fact]
    public async Task Upload_WithASingleLinkId_IsAccepted()
    {
        ServiceStores(SampleResponse());

        var response = await _sut.Upload(VehicleId, FileOf(), Guid.NewGuid(), null, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(response);
    }

    // ---------- upload result mapping ----------

    [Fact]
    public async Task Upload_WhenStored_ReturnsCreatedAtTheDocumentsUrlWithBothRouteValues()
    {
        var document = SampleResponse();
        ServiceStores(document);

        var response = await _sut.Upload(VehicleId, FileOf(), RecordId, null, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(nameof(DocumentsController.Get), created.ActionName);
        Assert.Equal(VehicleId, created.RouteValues!["vehicleId"]);
        Assert.Equal(document.Id, created.RouteValues["id"]);
        Assert.Same(document, created.Value);
    }

    [Fact]
    public async Task Upload_PassesTheOwnerFromTheTokenTheVehicleFromTheRouteAndTheFileMetadata()
    {
        UploadDocumentRequest? captured = null;
        var recordId = Guid.NewGuid();
        _service
            .Setup(s => s.UploadAsync(CallerId, VehicleId, It.IsAny<UploadDocumentRequest>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, UploadDocumentRequest, CancellationToken>((_, _, r, _) => captured = r)
            .ReturnsAsync(SampleResponse());

        await _sut.Upload(VehicleId, FileOf(fileName: "scan.pdf"), recordId, null, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("scan.pdf", captured!.FileName);
        Assert.Equal("application/pdf", captured.ContentType);
        Assert.Equal(3, captured.SizeBytes);
        Assert.Equal(recordId, captured.MaintenanceRecordId);
        Assert.Null(captured.ObligationId);
        _service.Verify(
            s => s.UploadAsync(CallerId, VehicleId, It.IsAny<UploadDocumentRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Upload_WhenTheVehicleOrLinkIsNotFound_ReturnsNotFound()
    {
        ServiceStores(null);

        var response = await _sut.Upload(VehicleId, FileOf(), RecordId, null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    // ---------- read ----------

    [Fact]
    public async Task List_WhenServiceReturnsDocuments_ReturnsOk()
    {
        _service
            .Setup(s => s.ListAsync(CallerId, VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([SampleResponse()]);

        var response = await _sut.List(VehicleId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        var documents = Assert.IsAssignableFrom<IReadOnlyList<DocumentResponse>>(ok.Value);
        Assert.Single(documents);
    }

    [Fact]
    public async Task List_WhenVehicleNotFound_ReturnsNotFound()
    {
        _service
            .Setup(s => s.ListAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<DocumentResponse>?)null);

        var response = await _sut.List(VehicleId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Get_WhenFound_ReturnsOk()
    {
        var document = SampleResponse();
        _service
            .Setup(s => s.GetAsync(CallerId, VehicleId, document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var response = await _sut.Get(VehicleId, document.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(document, ok.Value);
    }

    [Fact]
    public async Task Get_WhenMissing_ReturnsNotFound()
    {
        _service
            .Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentResponse?)null);

        var response = await _sut.Get(VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    // ---------- download ----------

    [Fact]
    public async Task Download_WhenFound_ReturnsTheFileWithItsTypeAndName()
    {
        var content = new MemoryStream([1, 2, 3]);
        _service
            .Setup(s => s.DownloadAsync(CallerId, VehicleId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDownload(content, "image/png", "sticker.png"));

        var response = await _sut.Download(VehicleId, Guid.NewGuid(), CancellationToken.None);

        var file = Assert.IsType<FileStreamResult>(response);
        Assert.Same(content, file.FileStream);
        Assert.Equal("image/png", file.ContentType);
        Assert.Equal("sticker.png", file.FileDownloadName);
    }

    [Fact]
    public async Task Download_WhenMissing_ReturnsNotFound()
    {
        _service
            .Setup(s => s.DownloadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentDownload?)null);

        var response = await _sut.Download(VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    // ---------- delete ----------

    [Fact]
    public async Task Delete_WhenDeleted_ReturnsNoContent()
    {
        _service
            .Setup(s => s.DeleteAsync(CallerId, VehicleId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _sut.Delete(VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task Delete_WhenNothingDeleted_ReturnsNotFound()
    {
        _service
            .Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var response = await _sut.Delete(VehicleId, Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }
}
