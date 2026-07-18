using System.ComponentModel.DataAnnotations;

namespace CarOrganizer.Application.Vehicles;

/// <summary>Payload for adding a vehicle to the caller's garage.</summary>
/// <remarks>
/// <see cref="CurrentMileage"/> is optional: leaving it out means "same as purchase" (a car you've
/// only just acquired). When supplied it must not be below <see cref="PurchaseMileage"/> — an
/// odometer doesn't run backwards. That cross-field rule can't be expressed with a single attribute,
/// so it lives in <see cref="Validate"/> and still surfaces as a normal 400.
/// </remarks>
public record CreateVehicleRequest(
    [Required, MaxLength(VehicleLimits.MakeMaxLength)] string Make,
    [Required, MaxLength(VehicleLimits.ModelMaxLength)] string Model,
    [Range(VehicleLimits.EarliestYear, VehicleLimits.LatestYear)] int Year,
    [Range(0, VehicleLimits.MaxMileage)] int PurchaseMileage,
    [Range(0, VehicleLimits.MaxMileage)] int? CurrentMileage,
    [MaxLength(VehicleLimits.VinMaxLength)] string? Vin,
    [MaxLength(VehicleLimits.RegistrationPlateMaxLength)] string? RegistrationPlate,
    [MaxLength(VehicleLimits.EngineMaxLength)] string? Engine) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        VehicleMileage.ValidateOrder(PurchaseMileage, CurrentMileage);
}
