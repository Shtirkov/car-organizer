using CarOrganizer.Domain.Common;
using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Domain.Entities;

/// <summary>
/// An upcoming event the owner should be reminded about (insurance, inspection, oil change...).
/// Can be triggered by a due date, a due mileage, or both.
/// </summary>
public class Reminder : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public ReminderType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Date the reminder is due, if date-based.</summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>Odometer reading (km) the reminder is due at, if mileage-based.</summary>
    public int? DueMileage { get; set; }

    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }
}
