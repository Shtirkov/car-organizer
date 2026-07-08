namespace CarOrganizer.Infrastructure.Identity;

/// <summary>Strongly-typed JWT configuration, bound from the "Jwt" configuration section.</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Token issuer (the "iss" claim) — our API.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Intended audience (the "aud" claim).</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Symmetric signing secret. Must be at least 32 bytes for HMAC-SHA256. Never commit the real value.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Access-token lifetime in minutes.</summary>
    public int AccessTokenMinutes { get; set; } = 15;
}
