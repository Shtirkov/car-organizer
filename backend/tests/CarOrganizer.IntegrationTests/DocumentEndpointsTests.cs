using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Documents;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.MaintenanceRecords;
using CarOrganizer.Application.Obligations;
using CarOrganizer.Application.Vehicles;
using CarOrganizer.Domain.Enums;
using CarOrganizer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// End-to-end tests for /api/vehicles/{vehicleId}/documents over the full HTTP pipeline, including
/// the real <c>LocalFileStorage</c> writing to a per-factory temp directory — so an upload/download
/// round trip proves the bytes actually survive a trip through storage, not just the metadata row.
/// Clients register and log in for real (so the vehicle's OwnerId is a genuine user, as the FK demands).
/// Every upload names exactly one maintenance record or obligation — paperwork of unknown purpose
/// is refused — so most tests set up a record alongside the vehicle.
/// </summary>
public class DocumentEndpointsTests : IDisposable
{
    private const string Password = "Passw0rd!23";

    /// <summary>A minimal but genuine JPEG: SOI, APP0/JFIF header, EOI.</summary>
    private static readonly byte[] JpegBytes =
    [
        0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00,
        0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9,
    ];

    private readonly CustomWebApplicationFactory _factory;

    public DocumentEndpointsTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<HttpClient> SignUpAsync(string email)
    {
        var client = _factory.CreateClient();
        var credentials = new { email, password = Password };

        await client.PostAsJsonAsync("/api/auth/register", credentials);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", credentials);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        return client;
    }

    private static async Task<VehicleResponse> CreateVehicleAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/vehicles",
            new { make = "Audi", model = "A4", year = 2015, purchaseMileage = 150_000, currentMileage = 190_000 });
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>())!;
    }

    private static async Task<MaintenanceRecordResponse> CreateRecordAsync(HttpClient client, Guid vehicleId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/vehicles/{vehicleId}/maintenance-records",
            new { type = (int)MaintenanceType.OilChange, date = "2026-06-01", mileage = 195_000, cost = 120.50 });
        return (await response.Content.ReadFromJsonAsync<MaintenanceRecordResponse>())!;
    }

    private static async Task<VehicleObligationResponse> CreateObligationAsync(HttpClient client, Guid vehicleId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/vehicles/{vehicleId}/obligations",
            new { type = (int)ObligationType.Insurance, validUntil = "2026-12-31", cost = 450.00 });
        return (await response.Content.ReadFromJsonAsync<VehicleObligationResponse>())!;
    }

    /// <summary>A vehicle plus a maintenance record on it — the minimum an upload needs.</summary>
    private static async Task<(VehicleResponse Vehicle, Guid RecordId)> VehicleWithRecordAsync(HttpClient client)
    {
        var vehicle = await CreateVehicleAsync(client);
        var record = await CreateRecordAsync(client, vehicle.Id);
        return (vehicle, record.Id);
    }

    private static string DocumentsUrl(Guid vehicleId) => $"/api/vehicles/{vehicleId}/documents";

    private static MultipartFormDataContent Upload(
        byte[]? bytes = null,
        string fileName = "invoice.pdf",
        string contentType = "application/pdf",
        Guid? maintenanceRecordId = null,
        Guid? obligationId = null)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes ?? JpegBytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);

        if (maintenanceRecordId is not null)
        {
            form.Add(new StringContent(maintenanceRecordId.Value.ToString()), "maintenanceRecordId");
        }

        if (obligationId is not null)
        {
            form.Add(new StringContent(obligationId.Value.ToString()), "obligationId");
        }

        return form;
    }

    private static async Task<DocumentResponse> UploadDocumentAsync(
        HttpClient client, Guid vehicleId, Guid? maintenanceRecordId = null, Guid? obligationId = null)
    {
        var response = await client.PostAsync(
            DocumentsUrl(vehicleId), Upload(maintenanceRecordId: maintenanceRecordId, obligationId: obligationId));
        return (await response.Content.ReadFromJsonAsync<DocumentResponse>())!;
    }

    /// <summary>Signs up, creates a vehicle + record, and uploads one document against that record.</summary>
    private async Task<(HttpClient Client, VehicleResponse Vehicle, DocumentResponse Document)> UploadedAsync(string email)
    {
        var client = await SignUpAsync(email);
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);
        var document = await UploadDocumentAsync(client, vehicle.Id, recordId);
        return (client, vehicle, document);
    }

    // ---------- authentication ----------

    [Fact]
    public async Task List_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(DocumentsUrl(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutAToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync(
            DocumentsUrl(Guid.NewGuid()), Upload(maintenanceRecordId: Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- upload ----------

    [Fact]
    public async Task Upload_WithAValidFile_ReturnsCreated()
    {
        using var client = await SignUpAsync("doc.create@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id), Upload(maintenanceRecordId: recordId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Upload_PointsLocationAtTheNewDocument()
    {
        using var client = await SignUpAsync("doc.location@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id), Upload(maintenanceRecordId: recordId));

        Assert.NotNull(response.Headers.Location);
        var followUp = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, followUp.StatusCode);
    }

    [Fact]
    public async Task Upload_PersistsTheMetadataAgainstTheVehicle()
    {
        using var client = await SignUpAsync("doc.persist@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);

        var created = await UploadDocumentAsync(client, vehicle.Id, recordId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var document = await db.Documents.SingleAsync();
        Assert.Equal(created.Id, document.Id);
        Assert.Equal(vehicle.Id, document.VehicleId);
        Assert.Equal("invoice.pdf", document.FileName);
        Assert.Equal("application/pdf", document.ContentType);
        Assert.Equal(JpegBytes.Length, document.SizeBytes);
        Assert.NotEmpty(document.StorageKey);
    }

    [Fact]
    public async Task Upload_DoesNotLeakTheVehicleIdOrStorageKey()
    {
        using var client = await SignUpAsync("doc.noleak@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id), Upload(maintenanceRecordId: recordId));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("vehicleId", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("storageKey", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_ForAnUnknownVehicle_ReturnsNotFound()
    {
        using var client = await SignUpAsync("doc.novehicle@example.com");

        var response = await client.PostAsync(
            DocumentsUrl(Guid.NewGuid()), Upload(maintenanceRecordId: Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- upload validation ----------

    [Theory]
    [InlineData("image/heic")]
    [InlineData("text/plain")]
    [InlineData("application/zip")]
    public async Task Upload_WithAnUnsupportedContentType_ReturnsBadRequest(string contentType)
    {
        using var client = await SignUpAsync($"doc.type{contentType.GetHashCode()}@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id),
            Upload(contentType: contentType, fileName: "photo.heic", maintenanceRecordId: recordId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithAnEmptyFile_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("doc.empty@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id), Upload([], maintenanceRecordId: recordId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithNoLink_ReturnsBadRequestNotNotFound()
    {
        using var client = await SignUpAsync("doc.nolink@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsync(DocumentsUrl(vehicle.Id), Upload());

        // A document must say what it is paperwork for. That is a malformed request, not a missing
        // resource, so the client gets a 400 that explains itself rather than a bare 404.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("maintenanceRecordId", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Upload_WithBothLinkIds_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("doc.bothlinks@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);
        var obligation = await CreateObligationAsync(client, vehicle.Id);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id), Upload(maintenanceRecordId: recordId, obligationId: obligation.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_LeavesNothingBehindWhenRejected()
    {
        using var client = await SignUpAsync("doc.noorphan@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);

        await client.PostAsync(DocumentsUrl(vehicle.Id), Upload(contentType: "image/heic", maintenanceRecordId: recordId));
        await client.PostAsync(DocumentsUrl(vehicle.Id), Upload(maintenanceRecordId: Guid.NewGuid()));
        await client.PostAsync(DocumentsUrl(vehicle.Id), Upload());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await db.Documents.ToListAsync());
    }

    // ---------- links ----------

    [Fact]
    public async Task Upload_LinkedToAMaintenanceRecord_PersistsTheLink()
    {
        using var client = await SignUpAsync("doc.recordlink@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);

        var created = await UploadDocumentAsync(client, vehicle.Id, recordId);

        Assert.Equal(recordId, created.MaintenanceRecordId);
        Assert.Null(created.VehicleObligationId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var document = await db.Documents.SingleAsync();
        Assert.Equal(recordId, document.MaintenanceRecordId);
    }

    [Fact]
    public async Task Upload_LinkedToAnObligation_PersistsTheLink()
    {
        using var client = await SignUpAsync("doc.obligationlink@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var obligation = await CreateObligationAsync(client, vehicle.Id);

        var created = await UploadDocumentAsync(client, vehicle.Id, obligationId: obligation.Id);

        Assert.Equal(obligation.Id, created.VehicleObligationId);
        Assert.Null(created.MaintenanceRecordId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var document = await db.Documents.SingleAsync();
        Assert.Equal(obligation.Id, document.VehicleObligationId);
    }

    [Fact]
    public async Task Upload_LinkedToAnUnknownMaintenanceRecord_ReturnsNotFound()
    {
        using var client = await SignUpAsync("doc.badrecordlink@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id), Upload(maintenanceRecordId: Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Upload_LinkedToARecordOnAnotherVehicle_ReturnsNotFound()
    {
        using var client = await SignUpAsync("doc.crossvehiclelink@example.com");
        var (first, recordOnFirst) = await VehicleWithRecordAsync(client);
        var second = await CreateVehicleAsync(client);

        var response = await client.PostAsync(
            DocumentsUrl(second.Id), Upload(maintenanceRecordId: recordOnFirst));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotEqual(first.Id, second.Id);
    }

    // ---------- download ----------

    [Fact]
    public async Task Download_ReturnsTheExactBytesThatWereUploaded()
    {
        var (client, vehicle, created) = await UploadedAsync("doc.roundtrip@example.com");
        using var _ = client;

        var response = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}/content");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal(JpegBytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Download_NamesTheFileInContentDisposition()
    {
        var (client, vehicle, created) = await UploadedAsync("doc.disposition@example.com");
        using var _ = client;

        var response = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}/content");

        Assert.Contains("invoice.pdf", response.Content.Headers.ContentDisposition!.ToString());
    }

    [Fact]
    public async Task Download_ForAnUnknownDocument_ReturnsNotFound()
    {
        using var client = await SignUpAsync("doc.downloadunknown@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{Guid.NewGuid()}/content");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- read ----------

    [Fact]
    public async Task Get_ReturnsTheDocumentMetadata()
    {
        var (client, vehicle, created) = await UploadedAsync("doc.get@example.com");
        using var _ = client;

        var response = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadFromJsonAsync<DocumentResponse>();
        Assert.Equal(created.Id, document!.Id);
        Assert.Equal("invoice.pdf", document.FileName);
    }

    [Fact]
    public async Task List_WithNoDocuments_ReturnsEmpty()
    {
        using var client = await SignUpAsync("doc.empty.list@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var documents = await client.GetFromJsonAsync<List<DocumentResponse>>(DocumentsUrl(vehicle.Id));

        Assert.Empty(documents!);
    }

    [Fact]
    public async Task List_ReturnsTheVehiclesDocuments()
    {
        using var client = await SignUpAsync("doc.list@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);
        await UploadDocumentAsync(client, vehicle.Id, recordId);
        await UploadDocumentAsync(client, vehicle.Id, recordId);

        var documents = await client.GetFromJsonAsync<List<DocumentResponse>>(DocumentsUrl(vehicle.Id));

        Assert.Equal(2, documents!.Count);
    }

    [Fact]
    public async Task List_DoesNotIncludeAnotherVehiclesDocuments()
    {
        using var client = await SignUpAsync("doc.perVehicle@example.com");
        var (first, recordId) = await VehicleWithRecordAsync(client);
        var second = await CreateVehicleAsync(client);
        await UploadDocumentAsync(client, first.Id, recordId);

        var documents = await client.GetFromJsonAsync<List<DocumentResponse>>(DocumentsUrl(second.Id));

        Assert.Empty(documents!);
    }

    [Fact]
    public async Task List_ForAnUnknownVehicle_ReturnsNotFound()
    {
        using var client = await SignUpAsync("doc.listunknown@example.com");

        var response = await client.GetAsync(DocumentsUrl(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- delete ----------

    [Fact]
    public async Task Delete_ReturnsNoContentAndTheDocumentIsGone()
    {
        var (client, vehicle, created) = await UploadedAsync("doc.delete@example.com");
        using var _ = client;

        var response = await client.DeleteAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var followUp = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, followUp.StatusCode);
    }

    [Fact]
    public async Task Delete_AlsoRemovesTheStoredBytes()
    {
        var (client, vehicle, created) = await UploadedAsync("doc.deletebytes@example.com");
        using var _ = client;

        await client.DeleteAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}");

        var download = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}/content");
        Assert.Equal(HttpStatusCode.NotFound, download.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await db.Documents.ToListAsync());
    }

    [Fact]
    public async Task Delete_ForAnUnknownDocument_ReturnsNotFound()
    {
        using var client = await SignUpAsync("doc.deleteunknown@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.DeleteAsync($"{DocumentsUrl(vehicle.Id)}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- cascade from the thing the document documents ----------

    [Fact]
    public async Task DeletingTheMaintenanceRecord_TakesItsDocumentsWithIt()
    {
        using var client = await SignUpAsync("doc.cascade.record@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);
        var document = await UploadDocumentAsync(client, vehicle.Id, recordId);

        var deleted = await client.DeleteAsync($"/api/vehicles/{vehicle.Id}/maintenance-records/{recordId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        // A document may never be paperwork of unknown purpose, so it goes with what it documented
        // rather than being detached.
        var followUp = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{document.Id}");
        Assert.Equal(HttpStatusCode.NotFound, followUp.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await db.Documents.ToListAsync());
    }

    [Fact]
    public async Task DeletingTheObligation_TakesItsDocumentsWithIt()
    {
        using var client = await SignUpAsync("doc.cascade.obligation@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var obligation = await CreateObligationAsync(client, vehicle.Id);
        var document = await UploadDocumentAsync(client, vehicle.Id, obligationId: obligation.Id);

        var deleted = await client.DeleteAsync($"/api/vehicles/{vehicle.Id}/obligations/{obligation.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var followUp = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{document.Id}");
        Assert.Equal(HttpStatusCode.NotFound, followUp.StatusCode);
    }

    [Fact]
    public async Task DeletingTheVehicle_TakesItsDocumentsWithIt()
    {
        using var client = await SignUpAsync("doc.cascade.vehicle@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);
        await UploadDocumentAsync(client, vehicle.Id, recordId);

        var deleted = await client.DeleteAsync($"/api/vehicles/{vehicle.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await db.Documents.ToListAsync());
    }

    [Fact]
    public async Task DeletingTheMaintenanceRecord_AlsoRemovesTheStoredFile()
    {
        using var client = await SignUpAsync("doc.cascade.bytes@example.com");
        var (vehicle, recordId) = await VehicleWithRecordAsync(client);
        var document = await UploadDocumentAsync(client, vehicle.Id, recordId);

        string storageKey;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            storageKey = (await db.Documents.SingleAsync(d => d.Id == document.Id)).StorageKey;
        }

        await client.DeleteAsync($"/api/vehicles/{vehicle.Id}/maintenance-records/{recordId}");

        // The database cascade only removes rows — the file has to be swept by us, or it is orphaned
        // on disk (and, after Phase 8, billed for in the bucket) forever.
        using var readBack = _factory.Services.CreateScope();
        var storage = readBack.ServiceProvider.GetRequiredService<IFileStorage>();
        Assert.Null(await storage.OpenReadAsync(storageKey, CancellationToken.None));
    }

    // ---------- isolation between users ----------

    [Fact]
    public async Task Upload_ToAnotherUsersVehicle_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("doc.alice.upload@example.com");
        using var bob = await SignUpAsync("doc.bob.upload@example.com");
        var (aliceVehicle, aliceRecordId) = await VehicleWithRecordAsync(alice);

        var response = await bob.PostAsync(
            DocumentsUrl(aliceVehicle.Id), Upload(maintenanceRecordId: aliceRecordId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_AnotherUsersDocument_ReturnsNotFound()
    {
        var (alice, aliceVehicle, aliceDocument) = await UploadedAsync("doc.alice.get@example.com");
        using var _ = alice;
        using var bob = await SignUpAsync("doc.bob.get@example.com");

        var response = await bob.GetAsync($"{DocumentsUrl(aliceVehicle.Id)}/{aliceDocument.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Download_AnotherUsersDocument_ReturnsNotFound()
    {
        var (alice, aliceVehicle, aliceDocument) = await UploadedAsync("doc.alice.download@example.com");
        using var _ = alice;
        using var bob = await SignUpAsync("doc.bob.download@example.com");

        var response = await bob.GetAsync($"{DocumentsUrl(aliceVehicle.Id)}/{aliceDocument.Id}/content");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AnotherUsersDocument_ReturnsNotFoundAndLeavesItAlone()
    {
        var (alice, aliceVehicle, aliceDocument) = await UploadedAsync("doc.alice.delete@example.com");
        using var _ = alice;
        using var bob = await SignUpAsync("doc.bob.delete@example.com");

        var response = await bob.DeleteAsync($"{DocumentsUrl(aliceVehicle.Id)}/{aliceDocument.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Still downloadable by its owner, so neither the row nor the bytes were touched.
        var download = await alice.GetAsync($"{DocumentsUrl(aliceVehicle.Id)}/{aliceDocument.Id}/content");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
    }
}
