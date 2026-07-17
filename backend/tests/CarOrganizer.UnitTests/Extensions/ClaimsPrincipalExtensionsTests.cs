using System.Security.Claims;
using CarOrganizer.API.Extensions;

namespace CarOrganizer.UnitTests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal PrincipalWith(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "Test"));

    [Fact]
    public void GetUserId_ReturnsTheGuidFromTheSubClaim()
    {
        var userId = Guid.NewGuid();

        var result = PrincipalWith(new Claim("sub", userId.ToString())).GetUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_ReadsSubRatherThanTheMappedNameIdentifier()
    {
        // MapInboundClaims is off, so the raw "sub" is the claim that carries the id. If a
        // NameIdentifier is also present, "sub" still wins.
        var subject = Guid.NewGuid();

        var result = PrincipalWith(
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("sub", subject.ToString())).GetUserId();

        Assert.Equal(subject, result);
    }

    [Fact]
    public void GetUserId_WithoutASubClaim_Throws()
    {
        var principal = PrincipalWith(new Claim("email", "user@example.com"));

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    [Fact]
    public void GetUserId_WithANonGuidSubClaim_Throws()
    {
        var principal = PrincipalWith(new Claim("sub", "not-a-guid"));

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }
}
