using System.Security.Claims;
using CarOrganizer.Application.Auth;
using CarOrganizer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarOrganizer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {

        //"email": "admin@admin.com",
        //"password": "Admin123@"
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return Unauthorized(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return Unauthorized(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);

        // 204 regardless of whether the token existed — idempotent, and doesn't reveal token validity.
        return NoContent();
    }

    /// <summary>
    /// Returns the identity carried by the current access token. Reaching this endpoint at all
    /// proves the token was present, correctly signed and unexpired (the middleware validated it).
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue("sub"),
            email = User.FindFirstValue("email"),
        });
    }
}
