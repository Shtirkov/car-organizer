using CarOrganizer.Domain.Common;
using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Domain.Entities;

/// <summary>
/// A time-bound legal or financial obligation on a vehicle — insurance, casco, technical inspection,
/// vignette or tax. Unlike a <see cref="MaintenanceRecord"/> (a past mechanical event), an obligation
/// has a validity period, and its <see cref="ValidUntil"/> is what upcoming-renewal reminders are
/// derived from.
/// </summary>
public class VehicleObligation : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public ObligationType Type { get; set; }

    /// <summary>Start of the validity period, if known.</summary>
    public DateOnly? ValidFrom { get; set; }

    /// <summary>End of the validity period — the date the obligation must be renewed by.</summary>
    public DateOnly ValidUntil { get; set; }

    /// <summary>What was paid for this obligation (premium, tax, fee...).</summary>
    public decimal Cost { get; set; }

    /// <summary>Insurer, testing station or issuing authority.</summary>
    public string? Provider { get; set; }

    /// <summary>Policy / sticker / certificate number.</summary>
    public string? PolicyNumber { get; set; }

    public string? Notes { get; set; }
}
