using CarOrganizer.Domain.Common;

namespace CarOrganizer.Domain.Entities;

/// <summary>
/// A vehicle in an owner's garage. Root for its maintenance records, documents and reminders.
/// </summary>
public class Vehicle : BaseEntity
{
    /// <summary>Identity user who owns the vehicle. Every query for a vehicle is scoped by this.</summary>
    public Guid OwnerId { get; set; }

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

    /// <summary>Odometer reading (km) when the owner acquired the vehicle. A fixed historical fact.</summary>
    public int PurchaseMileage { get; set; }

    /// <summary>
    /// Current odometer reading (km). Advanced automatically whenever a maintenance record is logged
    /// at a higher reading; never below <see cref="PurchaseMileage"/>.
    /// </summary>
    public int CurrentMileage { get; set; }

    public string? Vin { get; set; }
    public string? RegistrationPlate { get; set; }

    /// <summary>Free-text engine description (e.g. "2.0 TDI").</summary>
    public string? Engine { get; set; }

    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    public ICollection<VehicleObligation> Obligations { get; set; } = new List<VehicleObligation>();
}
