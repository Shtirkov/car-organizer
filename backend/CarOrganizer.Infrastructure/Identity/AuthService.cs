using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Common;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarOrganizer.Infrastructure.Identity;

/// <summary>
/// Identity-backed implementation of <see cref="IAuthService"/>. Delegates the security-sensitive
/// work (password hashing, uniqueness, validation) to ASP.NET Identity's <see cref="UserManager{TUser}"/>.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(UserManager<User> userManager, IJwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
        };

        // CreateAsync hashes the password and enforces the configured user/password rules.
        var result = await _userManager.CreateAsync(user, request.Password);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Same error whether the email is unknown or the password is wrong, so we don't reveal which accounts exist.
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Result<AuthResponse>.Failure(["Invalid email or password."]);
        }

        var accessToken = _tokenGenerator.GenerateAccessToken(user);

        return Result<AuthResponse>.Success(
            new AuthResponse(accessToken.Value, accessToken.ExpiresAtUtc));
    }
}
