namespace CarOrganizer.Domain.Enums;

/// <summary>
/// Category of a time-bound legal or financial obligation on a vehicle.
/// </summary>
public enum ObligationType
{
    Other = 0,

    /// <summary>Mandatory third-party liability insurance ("Гражданска отговорност").</summary>
    Insurance = 1,

    /// <summary>Optional comprehensive insurance ("Каско").</summary>
    Casco = 2,

    /// <summary>Periodic roadworthiness test ("Технически преглед").</summary>
    TechnicalInspection = 3,

    /// <summary>Road-tax sticker ("Винетка").</summary>
    Vignette = 4,

    /// <summary>Annual vehicle tax ("Данък МПС").</summary>
    Tax = 5,
}
