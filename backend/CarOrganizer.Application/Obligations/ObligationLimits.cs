namespace CarOrganizer.Application.Obligations;

/// <summary>
/// Bounds shared by the obligation request DTOs, kept here so create and update can't drift apart
/// and so the attribute limits stay compile-time constants.
/// </summary>
public static class ObligationLimits
{
    public const int ProviderMaxLength = 120;
    public const int PolicyNumberMaxLength = 80;

    /// <summary>Matches the column length in <c>VehicleObligationConfiguration</c>.</summary>
    public const int NotesMaxLength = 2000;

    /// <summary>A premium/tax/fee beyond this is a typo, not a bill.</summary>
    public const int MaxCost = 1_000_000;
}
