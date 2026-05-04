using System.Security.Claims;
using CarRental.Application.DTOs.Auth;
using CarRental.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Register a new client account</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Login), result);
    }

    /// <summary>Refresh access token using refresh token</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
        return Ok(result);
    }

    /// <summary>Logout and revoke refresh token</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    /// <summary>Change password for authenticated user</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        await _authService.ChangePasswordAsync(userId, request, ct);
        return NoContent();
    }

    /// <summary>Get current user info from token</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), 200)]
    public IActionResult Me()
    {
        return Ok(new UserInfoDto(
            Id: int.Parse(User.FindFirstValue("userId")!),
            FullName: User.FindFirstValue("fullName")!,
            Username: User.FindFirstValue(ClaimTypes.Name)!,
            Email: User.FindFirstValue(ClaimTypes.Email)!,
            Phone: null,
            Role: User.FindFirstValue(ClaimTypes.Role)!,
            BranchId: User.FindFirstValue("branchId") is string b ? int.Parse(b) : null
        ));
    }
}
