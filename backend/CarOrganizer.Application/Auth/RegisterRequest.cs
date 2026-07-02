using System.ComponentModel.DataAnnotations;

namespace CarOrganizer.Application.Auth;

/// <summary>Payload for registering a new account.</summary>
public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);
