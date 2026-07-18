using System.ComponentModel.DataAnnotations;

namespace CarOrganizer.Application.Vehicles;

/// <summary>
/// Payload for editing a vehicle. A full replacement (PUT): every field is written, so omitting
/// an optional one clears it. Both mileages are given explicitly — <see cref="CurrentMileage"/> may
/// be corrected here (e.g. to fix an odometer typo), but never set below <see cref="PurchaseMileage"/>.
/// </summary>
public record UpdateVehicleRequest(
    [Required, MaxLength(VehicleLimits.MakeMaxLength)] string Make,
    [Required, MaxLength(VehicleLimits.ModelMaxLength)] string Model,
    [Range(VehicleLimits.EarliestYear, VehicleLimits.LatestYear)] int Year,
    [Range(0, VehicleLimits.MaxMileage)] int PurchaseMileage,
    [Range(0, VehicleLimits.MaxMileage)] int CurrentMileage,
    [MaxLength(VehicleLimits.VinMaxLength)] string? Vin,
    [MaxLength(VehicleLimits.RegistrationPlateMaxLength)] string? RegistrationPlate,
    [MaxLength(VehicleLimits.EngineMaxLength)] string? Engine) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        VehicleMileage.ValidateOrder(PurchaseMileage, CurrentMileage);
}
