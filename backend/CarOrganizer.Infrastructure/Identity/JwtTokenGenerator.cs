using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CarOrganizer.Infrastructure.Identity;

/// <summary>
/// Builds signed JWT access tokens. The payload carries the user's id (<c>sub</c>), email and a
/// unique token id (<c>jti</c>); it is signed with a symmetric key using HMAC-SHA256 so the API
/// can later verify it was not tampered with.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public AccessToken GenerateAccessToken(User user)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(tokenValue, expiresAt);
    }
}
