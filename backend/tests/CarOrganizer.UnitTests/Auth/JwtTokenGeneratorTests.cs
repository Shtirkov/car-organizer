using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CarOrganizer.Domain.Entities;
using CarOrganizer.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CarOrganizer.UnitTests.Auth;

public class JwtTokenGeneratorTests
{
    private static readonly JwtSettings Settings = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        Key = "test-signing-key-that-is-at-least-32-bytes-long-0123456789",
        AccessTokenMinutes = 15,
    };

    private static JwtTokenGenerator CreateSut() => new(Options.Create(Settings));

    private static TokenValidationParameters ValidationParameters(string? key = null) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = Settings.Issuer,
        ValidateAudience = true,
        ValidAudience = Settings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? Settings.Key)),
        ValidateLifetime = false,
    };

    [Fact]
    public void GenerateAccessToken_ProducesAThreeSegmentJwt()
    {
        var token = CreateSut().GenerateAccessToken(new User { Email = "user@example.com" });

        Assert.Equal(3, token.Value.Split('.').Length);
    }

    [Fact]
    public void GenerateAccessToken_PutsUserIdInSubAndEmailInEmailClaim()
    {
        var user = new User { Email = "user@example.com" };

        var token = CreateSut().GenerateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("user@example.com", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public void GenerateAccessToken_SetsIssuerAndAudience()
    {
        var token = CreateSut().GenerateAccessToken(new User { Email = "user@example.com" });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);
        Assert.Equal(Settings.Issuer, jwt.Issuer);
        Assert.Contains(Settings.Audience, jwt.Audiences);
    }

    [Fact]
    public void GenerateAccessToken_SetsExpiryAboutAccessTokenMinutesFromNow()
    {
        var before = DateTime.UtcNow;

        var token = CreateSut().GenerateAccessToken(new User { Email = "user@example.com" });

        var expected = before.AddMinutes(Settings.AccessTokenMinutes);
        Assert.InRange(token.ExpiresAtUtc, expected.AddSeconds(-5), expected.AddSeconds(30));
    }

    [Fact]
    public void GenerateAccessToken_GivesEachTokenAUniqueJti()
    {
        var sut = CreateSut();
        var user = new User { Email = "user@example.com" };

        var first = new JwtSecurityTokenHandler().ReadJwtToken(sut.GenerateAccessToken(user).Value);
        var second = new JwtSecurityTokenHandler().ReadJwtToken(sut.GenerateAccessToken(user).Value);

        var firstJti = first.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var secondJti = second.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        Assert.NotEqual(firstJti, secondJti);
    }

    [Fact]
    public void GenerateAccessToken_IsSignedWithTheConfiguredKey()
    {
        var token = CreateSut().GenerateAccessToken(new User { Email = "user@example.com" });

        // ValidateToken throws unless the signature checks out against the key.
        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(token.Value, ValidationParameters(), out var validated);

        Assert.NotNull(principal);
        Assert.NotNull(validated);
    }

    [Fact]
    public void GenerateAccessToken_FailsValidationWhenVerifiedWithAWrongKey()
    {
        var token = CreateSut().GenerateAccessToken(new User { Email = "user@example.com" });

        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(
                token.Value,
                ValidationParameters("a-totally-different-key-that-is-also-32-bytes-long"),
                out _));
    }
}
