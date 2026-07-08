namespace CarOrganizer.Application.Auth;

/// <summary>Tokens returned to the client after a successful login.</summary>
public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, string TokenType = "Bearer");
