using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Common;

namespace CarOrganizer.Application.Interfaces;

/// <summary>Account/authentication operations. Implemented in the Infrastructure layer.</summary>
public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
