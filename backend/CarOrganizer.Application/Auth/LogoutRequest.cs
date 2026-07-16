using System.ComponentModel.DataAnnotations;

namespace CarOrganizer.Application.Auth;

/// <summary>Payload for revoking a refresh token (logging out a session).</summary>
public record LogoutRequest([Required] string RefreshToken);
