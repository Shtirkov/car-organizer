namespace CarOrganizer.Application.Vehicles;

/// <summary>
/// Bounds shared by the vehicle request DTOs. They live here rather than inline because validation
/// attributes need compile-time constants, and Create/Update must not drift apart.
/// </summary>
public static class VehicleLimits
{
    public const int MakeMaxLength = 80;
    public const int ModelMaxLength = 80;
    public const int EngineMaxLength = 80;

    /// <summary>Modern VINs are exactly 17 characters; older vehicles have shorter ones.</summary>
    public const int VinMaxLength = 17;

    public const int RegistrationPlateMaxLength = 20;

    /// <summary>Roughly the dawn of the production car — anything earlier is a typo, not a vehicle.</summary>
    public const int EarliestYear = 1900;

    /// <summary>Deliberately loose: a fixed constant can't track "next model year", and being
    /// wrong here would reject a legitimate new car. The far bound only catches nonsense.</summary>
    public const int LatestYear = 2100;

    /// <summary>2 million km — beyond any real odometer, so this only rejects garbage input.</summary>
    public const int MaxMileage = 2_000_000;
}
