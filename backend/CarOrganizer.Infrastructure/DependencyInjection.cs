using System.Text;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Infrastructure.Authentication;
using CarOrganizer.Infrastructure.Dashboard;
using CarOrganizer.Infrastructure.Documents;
using CarOrganizer.Infrastructure.Identity;
using CarOrganizer.Infrastructure.MaintenanceRecords;
using CarOrganizer.Infrastructure.Obligations;
using CarOrganizer.Infrastructure.Persistence;
using CarOrganizer.Infrastructure.Storage;
using CarOrganizer.Infrastructure.Vehicles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarOrganizer.Infrastructure;

/// <summary>
/// Registers Infrastructure-layer services (EF Core, external integrations) into the container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AppDbContext>();

        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtKey = jwtSection[nameof(JwtSettings.Key)];
        if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
        {
            throw new InvalidOperationException("Jwt:Key is missing or too short. Provide a signing key of at least 32 bytes ");
        }

        services.Configure<JwtSettings>(jwtSection);
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IVehicleStore, VehicleStore>();
        services.AddScoped<IVehicleService, VehicleService>();

        services.AddScoped<IMaintenanceRecordStore, MaintenanceRecordStore>();
        services.AddScoped<IMaintenanceRecordService, MaintenanceRecordService>();

        services.AddScoped<IVehicleObligationStore, VehicleObligationStore>();
        services.AddScoped<IVehicleObligationService, VehicleObligationService>();

        services.AddScoped<IDashboardService, DashboardService>();

        services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IDocumentStore, DocumentStore>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IReminderStore, ReminderStore>();

        services.AddJwtAuthentication(configuration);

        return services;
    }
}
