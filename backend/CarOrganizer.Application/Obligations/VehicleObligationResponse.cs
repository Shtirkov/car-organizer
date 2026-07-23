using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Application.Obligations;

/// <summary>
/// An obligation as returned to the owner. Carries no <c>VehicleId</c> — the vehicle is already
/// fixed by the URL the obligation was reached through.
/// </summary>
public record VehicleObligationResponse(
    Guid Id,
    ObligationType Type,
    DateOnly? ValidFrom,
    DateOnly ValidUntil,
    decimal Cost,
    string? Provider,
    string? PolicyNumber,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
