namespace CarOrganizer.Domain.Enums;

/// <summary>
/// Category of an upcoming event the owner should be reminded about.
/// Reminders can be date-based, mileage-based, or both.
/// </summary>
public enum ReminderType
{
    Other = 0,
    Insurance = 1,
    TechnicalInspection = 2,
    OilChange = 3,
    Service = 4,
    TireChange = 5,
    Vignette = 6,
}
