using System.ComponentModel.DataAnnotations;

namespace CarOrganizer.Application.Auth;

/// <summary>Payload for exchanging a refresh token for a new access/refresh token pair.</summary>
public record RefreshRequest([Required] string RefreshToken);
