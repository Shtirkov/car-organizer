namespace CarOrganizer.Application.Auth;

/// <summary>A signed access token together with the moment it expires (UTC).</summary>
public record AccessToken(string Value, DateTime ExpiresAtUtc);
