using System.ComponentModel.DataAnnotations;
using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Application.Obligations;

/// <summary>
/// Payload for editing an obligation. A full replacement (PUT): every field is written, so omitting
/// an optional one clears it.
/// </summary>
public record UpdateVehicleObligationRequest(
    [EnumDataType(typeof(ObligationType))] ObligationType Type,
    DateOnly? ValidFrom,
    DateOnly ValidUntil,
    [Range(0, ObligationLimits.MaxCost)] decimal Cost,
    [MaxLength(ObligationLimits.ProviderMaxLength)] string? Provider,
    [MaxLength(ObligationLimits.PolicyNumberMaxLength)] string? PolicyNumber,
    [MaxLength(ObligationLimits.NotesMaxLength)] string? Notes) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        ObligationValidity.ValidateOrder(ValidFrom, ValidUntil);
}
