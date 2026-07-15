using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CarOrganizer.IntegrationTests;

/// <summary>
/// Forges JWTs for tests so we can exercise the validation middleware directly, without needing
/// to register/login first. Defaults produce a token the app accepts; override a parameter to
/// simulate an expired, wrong-key, or wrong-issuer token.
/// </summary>
public static class TestJwt
{
    public static string Create(
        string? sub = null,
        string? email = null,
        DateTime? expires = null,
        string key = CustomWebApplicationFactory.JwtKey,
        string issuer = CustomWebApplicationFactory.JwtIssuer,
        string audience = CustomWebApplicationFactory.JwtAudience)
    {
        var claims = new List<Claim>();
        if (sub is not null) claims.Add(new Claim(JwtRegisteredClaimNames.Sub, sub));
        if (email is not null) claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-5),
            expires: expires ?? DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
