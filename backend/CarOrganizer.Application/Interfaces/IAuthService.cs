using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Common;

namespace CarOrganizer.Application.Interfaces;

/// <summary>Account/authentication operations. Implemented in the Infrastructure layer.</summary>
public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Validates credentials and, on success, issues an access + refresh token pair.</summary>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Exchanges a valid refresh token for a new token pair, rotating the refresh token.</summary>
    Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);
}
