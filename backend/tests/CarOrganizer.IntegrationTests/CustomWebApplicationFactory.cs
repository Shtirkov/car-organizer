using CarOrganizer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// Boots the real API for end-to-end tests, but swaps the PostgreSQL <see cref="AppDbContext"/>
/// registration for an isolated in-memory database so tests need no external server.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Exposed so tests can forge tokens (valid, expired, wrong-key) that match this app's config.
    public const string JwtIssuer = "test-issuer";
    public const string JwtAudience = "test-audience";
    public const string JwtKey = "integration-test-signing-key-at-least-32-bytes-long-0123456789";

    private readonly string _databaseName = "CarOrganizerTests-" + Guid.NewGuid();
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "CarOrganizerTests-" + Guid.NewGuid());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // AddInfrastructure throws if this is missing; the Npgsql registration it creates is replaced below.
        builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=dummy;Username=dummy;Password=dummy");

        // Self-contained JWT config so the app boots without relying on appsettings.Development.json.
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:AccessTokenMinutes", "15");

        // The real LocalFileStorage runs, just pointed at a throwaway directory — uploads are then
        // exercised end to end (bytes actually hit a disk) without leaking into the repository.
        builder.UseSetting("Storage:LocalRoot", _storageRoot);

        builder.ConfigureTestServices(services =>
        {
            // Drop every descriptor tied to the real DbContext/provider, then re-register on InMemory.
            // The provider is applied through IDbContextOptionsConfiguration<AppDbContext>; leaving the
            // Npgsql one in place would apply two providers to the same options and throw.
            var descriptorsToRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // The in-memory database goes away with the factory; the uploaded files don't, so they are
        // swept here rather than accumulating in the temp directory across runs.
        if (disposing && Directory.Exists(_storageRoot))
        {
            Directory.Delete(_storageRoot, recursive: true);
        }
    }
}
