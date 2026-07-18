using System.ComponentModel.DataAnnotations;

namespace CarOrganizer.Application.Vehicles;

/// <summary>
/// The one invariant tying a vehicle's two odometer readings together, shared by the create and
/// update payloads so they can't drift apart.
/// </summary>
public static class VehicleMileage
{
    public const string CurrentBelowPurchaseMessage =
        "CurrentMileage cannot be lower than PurchaseMileage — an odometer does not run backwards.";

    /// <summary>
    /// Yields a validation error when a supplied current reading is below the purchase reading.
    /// A <c>null</c> current reading is fine — the caller is saying "same as purchase".
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateOrder(int purchaseMileage, int? currentMileage)
    {
        if (currentMileage.HasValue && currentMileage.Value < purchaseMileage)
        {
            yield return new ValidationResult(CurrentBelowPurchaseMessage, ["CurrentMileage"]);
        }
    }
}
