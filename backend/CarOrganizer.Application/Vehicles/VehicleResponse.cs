namespace CarOrganizer.Application.Vehicles;

/// <summary>
/// A vehicle as returned to its owner. Carries no <c>OwnerId</c> — the caller is always the owner,
/// so echoing a user id back would leak an identifier for nothing.
/// </summary>
public record VehicleResponse(
    Guid Id,
    string Make,
    string Model,
    int Year,
    int Mileage,
    string? Vin,
    string? RegistrationPlate,
    string? Engine,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
