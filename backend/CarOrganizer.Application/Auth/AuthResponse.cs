namespace CarOrganizer.Application.Auth;

/// <summary>Tokens returned to the client after a successful login or refresh.</summary>
public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string TokenType = "Bearer");
