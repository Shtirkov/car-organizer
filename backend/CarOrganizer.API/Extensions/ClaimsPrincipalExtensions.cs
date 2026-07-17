using System.Security.Claims;

namespace CarOrganizer.API.Extensions;

/// <summary>Reads the caller's identity out of the already-validated access token.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The caller's user id, from the token's <c>sub</c> claim.
    /// </summary>
    /// <remarks>
    /// Only call this from an <c>[Authorize]</c>-protected action. There, the claim is guaranteed
    /// to be present and to parse: the request reached the action at all only because
    /// <c>JwtBearerHandler</c> verified our own signature, and <c>JwtTokenGenerator</c> always
    /// writes <c>sub</c> as the user's Guid. The throw below is therefore an assertion about our
    /// own code, not input validation — a forged token never gets this far.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The principal carries no usable <c>sub</c> claim.</exception>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue("sub");

        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new InvalidOperationException(
                $"Expected a Guid 'sub' claim on the access token but found '{subject ?? "<none>"}'.");
    }
}
