using System.ComponentModel.DataAnnotations;

namespace CarOrganizer.Application.Auth;

/// <summary>Credentials for logging in and obtaining an access token.</summary>
public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
