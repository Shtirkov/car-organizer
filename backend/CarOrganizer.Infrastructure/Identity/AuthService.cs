using System.Security.Cryptography;
using System.Text;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Common;
using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CarOrganizer.Infrastructure.Identity;

/// <summary>
/// Identity-backed implementation of <see cref="IAuthService"/>. Delegates the security-sensitive
/// work (password hashing, uniqueness, validation) to ASP.NET Identity's <see cref="UserManager{TUser}"/>,
/// and manages the persisted refresh tokens itself.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<User> userManager,
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
        _jwtSettings = jwtSettings.Value;
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

        var tokens = await IssueTokensAsync(user, cancellationToken);
        return Result<AuthResponse>.Success(tokens);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        // We only ever store the hash, so hash the incoming raw token to look it up.
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _refreshTokenStore.FindByHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return Result<AuthResponse>.Failure(["Invalid or expired refresh token."]);
        }

        // Rotation: the presented refresh token is single-use — revoke it before issuing the next pair.
        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await _refreshTokenStore.UpdateAsync(storedToken, cancellationToken);

        var tokens = await IssueTokensAsync(storedToken.User, cancellationToken);
        return Result<AuthResponse>.Success(tokens);
    }

    /// <summary>Generates a fresh access token and a new persisted (hashed) refresh token for the user.</summary>
    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenGenerator.GenerateAccessToken(user);

        var rawRefreshToken = GenerateSecureToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawRefreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
        };
        await _refreshTokenStore.AddAsync(refreshToken, cancellationToken);

        // The client receives the RAW refresh token; only its hash is ever stored.
        return new AuthResponse(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            rawRefreshToken,
            refreshToken.ExpiresAtUtc);
    }

    /// <summary>256 bits of cryptographically-strong randomness, hex-encoded — the raw refresh token.</summary>
    private static string GenerateSecureToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    /// <summary>Fast hash (SHA-256) — fine here because the token is high-entropy, unlike a password.</summary>
    private static string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
