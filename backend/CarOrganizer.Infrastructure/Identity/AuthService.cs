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

    public AuthService(UserManager<User> userManager)
    {
        _userManager = userManager;
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
}
