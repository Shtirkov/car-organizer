using CarOrganizer.Domain.Common;
using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Domain.Entities;

/// <summary>
/// A single maintenance/service event performed on a vehicle.
/// </summary>
public class MaintenanceRecord : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public MaintenanceType Type { get; set; }

    /// <summary>Date the service was performed.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Odometer reading (km) at the time of the service.</summary>
    public int Mileage { get; set; }

    /// <summary>Total cost of the service.</summary>
    public decimal Cost { get; set; }

    public string? Notes { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
