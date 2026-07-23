using System.ComponentModel.DataAnnotations;
using CarOrganizer.Domain.Enums;

namespace CarOrganizer.Application.Obligations;

/// <summary>Payload for recording an obligation (insurance, tax, ...) on one of the caller's vehicles.</summary>
/// <remarks>
/// <see cref="ValidFrom"/> is optional; when supplied it must not be after <see cref="ValidUntil"/>.
/// That cross-field rule can't be expressed with a single attribute, so it lives in
/// <see cref="Validate"/> and still surfaces as a normal 400.
/// </remarks>
public record CreateVehicleObligationRequest(
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
