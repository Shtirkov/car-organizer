using CarOrganizer.Application.Auth;
using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Application.Interfaces;

/// <summary>Creates signed JWT access tokens for authenticated users.</summary>
public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(User user);
}
