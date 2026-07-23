using System.ComponentModel.DataAnnotations;

namespace CarOrganizer.Application.Obligations;

/// <summary>
/// The invariant tying an obligation's two validity dates together, shared by the create and update
/// payloads so they can't drift apart.
/// </summary>
public static class ObligationValidity
{
    public const string ValidFromAfterUntilMessage =
        "ValidFrom cannot be after ValidUntil — a validity period cannot end before it starts.";

    /// <summary>
    /// Yields a validation error when a supplied start date is after the end date. A <c>null</c>
    /// start is fine — the caller only knows (or cares about) the expiry.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateOrder(DateOnly? validFrom, DateOnly validUntil)
    {
        if (validFrom is DateOnly from && from > validUntil)
        {
            yield return new ValidationResult(ValidFromAfterUntilMessage, ["ValidFrom"]);
        }
    }
}
