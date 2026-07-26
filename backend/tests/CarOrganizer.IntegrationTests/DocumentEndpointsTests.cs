using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Documents;
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

        var response = await client.PostAsync(DocumentsUrl(Guid.NewGuid()), Upload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- upload ----------

    [Fact]
    public async Task Upload_WithAValidFile_ReturnsCreated()
    {
        using var client = await SignUpAsync("doc.create@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsync(DocumentsUrl(vehicle.Id), Upload());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Upload_PointsLocationAtTheNewDocument()
    {
        using var client = await SignUpAsync("doc.location@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsync(DocumentsUrl(vehicle.Id), Upload());

        Assert.NotNull(response.Headers.Location);
        var followUp = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, followUp.StatusCode);
    }

    [Fact]
    public async Task Upload_PersistsTheMetadataAgainstTheVehicle()
    {
        using var client = await SignUpAsync("doc.persist@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var created = await UploadDocumentAsync(client, vehicle.Id);

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
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsync(DocumentsUrl(vehicle.Id), Upload());
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("vehicleId", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("storageKey", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_ForAnUnknownVehicle_ReturnsNotFound()
    {
        using var client = await SignUpAsync("doc.novehicle@example.com");

        var response = await client.PostAsync(DocumentsUrl(Guid.NewGuid()), Upload());

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
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id), Upload(contentType: contentType, fileName: "photo.heic"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithAnEmptyFile_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("doc.empty@example.com");
        var vehicle = await CreateVehicleAsync(client);

        var response = await client.PostAsync(DocumentsUrl(vehicle.Id), Upload([]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithBothLinkIds_ReturnsBadRequest()
    {
        using var client = await SignUpAsync("doc.bothlinks@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var record = await CreateRecordAsync(client, vehicle.Id);
        var obligation = await CreateObligationAsync(client, vehicle.Id);

        var response = await client.PostAsync(
            DocumentsUrl(vehicle.Id), Upload(maintenanceRecordId: record.Id, obligationId: obligation.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_LeavesNoFileBehindWhenRejected()
    {
        using var client = await SignUpAsync("doc.noorphan@example.com");
        var vehicle = await CreateVehicleAsync(client);

        await client.PostAsync(DocumentsUrl(vehicle.Id), Upload(contentType: "image/heic"));
        await client.PostAsync(DocumentsUrl(vehicle.Id), Upload(maintenanceRecordId: Guid.NewGuid()));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await db.Documents.ToListAsync());
    }

    // ---------- links ----------

    [Fact]
    public async Task Upload_LinkedToAMaintenanceRecord_PersistsTheLink()
    {
        using var client = await SignUpAsync("doc.recordlink@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var record = await CreateRecordAsync(client, vehicle.Id);

        var created = await UploadDocumentAsync(client, vehicle.Id, maintenanceRecordId: record.Id);

        Assert.Equal(record.Id, created.MaintenanceRecordId);
        Assert.Null(created.VehicleObligationId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var document = await db.Documents.SingleAsync();
        Assert.Equal(record.Id, document.MaintenanceRecordId);
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
        var first = await CreateVehicleAsync(client);
        var second = await CreateVehicleAsync(client);
        var recordOnFirst = await CreateRecordAsync(client, first.Id);

        var response = await client.PostAsync(
            DocumentsUrl(second.Id), Upload(maintenanceRecordId: recordOnFirst.Id));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- download ----------

    [Fact]
    public async Task Download_ReturnsTheExactBytesThatWereUploaded()
    {
        using var client = await SignUpAsync("doc.roundtrip@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var created = await UploadDocumentAsync(client, vehicle.Id);

        var response = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}/content");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal(JpegBytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Download_NamesTheFileInContentDisposition()
    {
        using var client = await SignUpAsync("doc.disposition@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var created = await UploadDocumentAsync(client, vehicle.Id);

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
        using var client = await SignUpAsync("doc.get@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var created = await UploadDocumentAsync(client, vehicle.Id);

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
        var vehicle = await CreateVehicleAsync(client);
        await UploadDocumentAsync(client, vehicle.Id);
        await UploadDocumentAsync(client, vehicle.Id);

        var documents = await client.GetFromJsonAsync<List<DocumentResponse>>(DocumentsUrl(vehicle.Id));

        Assert.Equal(2, documents!.Count);
    }

    [Fact]
    public async Task List_DoesNotIncludeAnotherVehiclesDocuments()
    {
        using var client = await SignUpAsync("doc.perVehicle@example.com");
        var first = await CreateVehicleAsync(client);
        var second = await CreateVehicleAsync(client);
        await UploadDocumentAsync(client, first.Id);

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
        using var client = await SignUpAsync("doc.delete@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var created = await UploadDocumentAsync(client, vehicle.Id);

        var response = await client.DeleteAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var followUp = await client.GetAsync($"{DocumentsUrl(vehicle.Id)}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, followUp.StatusCode);
    }

    [Fact]
    public async Task Delete_AlsoRemovesTheStoredBytes()
    {
        using var client = await SignUpAsync("doc.deletebytes@example.com");
        var vehicle = await CreateVehicleAsync(client);
        var created = await UploadDocumentAsync(client, vehicle.Id);

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

    // ---------- isolation between users ----------

    [Fact]
    public async Task Upload_ToAnotherUsersVehicle_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("doc.alice.upload@example.com");
        using var bob = await SignUpAsync("doc.bob.upload@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);

        var response = await bob.PostAsync(DocumentsUrl(aliceVehicle.Id), Upload());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_AnotherUsersDocument_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("doc.alice.get@example.com");
        using var bob = await SignUpAsync("doc.bob.get@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        var aliceDocument = await UploadDocumentAsync(alice, aliceVehicle.Id);

        var response = await bob.GetAsync($"{DocumentsUrl(aliceVehicle.Id)}/{aliceDocument.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Download_AnotherUsersDocument_ReturnsNotFound()
    {
        using var alice = await SignUpAsync("doc.alice.download@example.com");
        using var bob = await SignUpAsync("doc.bob.download@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        var aliceDocument = await UploadDocumentAsync(alice, aliceVehicle.Id);

        var response = await bob.GetAsync($"{DocumentsUrl(aliceVehicle.Id)}/{aliceDocument.Id}/content");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AnotherUsersDocument_ReturnsNotFoundAndLeavesItAlone()
    {
        using var alice = await SignUpAsync("doc.alice.delete@example.com");
        using var bob = await SignUpAsync("doc.bob.delete@example.com");
        var aliceVehicle = await CreateVehicleAsync(alice);
        var aliceDocument = await UploadDocumentAsync(alice, aliceVehicle.Id);

        var response = await bob.DeleteAsync($"{DocumentsUrl(aliceVehicle.Id)}/{aliceDocument.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Still downloadable by its owner, so neither the row nor the bytes were touched.
        var download = await alice.GetAsync($"{DocumentsUrl(aliceVehicle.Id)}/{aliceDocument.Id}/content");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
    }
}
