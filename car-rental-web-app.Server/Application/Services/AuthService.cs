using CarRental.Application.DTOs.Auth;
using CarRental.Application.Interfaces;
using CarRental.Application.Mappings;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CarRental.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IAuditService _audit;

    public AuthService(IUnitOfWork uow, IJwtService jwt, IPasswordHasher<User> hasher, IAuditService audit)
    {
        _uow = uow;
        _jwt = jwt;
        _hasher = hasher;
        _audit = audit;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        if (user.IsCurrentlyLocked())
            throw new InvalidOperationException("Account is locked. Please try again later or contact support.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            user.RecordFailedLogin();

            // Create security alert after 5 failures
            if (user.FailedLoginAttempts >= 5)
            {
                var alert = SecurityAlert.Create(user.Id, AlertType.AccountLocked,
                    $"Account locked after {user.FailedLoginAttempts} failed login attempts.");
                await _uow.SecurityAlerts.AddAsync(alert, ct);
            }

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.RecordLogin();

        var accessToken = _jwt.GenerateAccessToken(user);
        var rawRefresh = _jwt.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(user.Id, rawRefresh, DateTime.UtcNow.AddDays(7));

        await _uow.RefreshTokens.AddAsync(refreshToken, ct);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(user.Id, "User", user.Id, "Login", $"User {user.Email} logged in.", ct);

        return new AuthResponse(accessToken, rawRefresh, _jwt.AccessTokenExpiresInSeconds, user.ToInfoDto());
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.EmailExistsAsync(request.Email, ct))
            throw new InvalidOperationException("Email already registered.");

        if (await _uow.Users.UsernameExistsAsync(request.Username, ct))
            throw new InvalidOperationException("Username already taken.");

        var tempUser = User.Create(request.Username, "", request.Email, request.FullName, request.Phone, UserRole.Operator);
        var hash = _hasher.HashPassword(tempUser, request.Password);

        var user = User.Create(request.Username, hash, request.Email, request.FullName, request.Phone, UserRole.Operator);
        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        var accessToken = _jwt.GenerateAccessToken(user);
        var rawRefresh = _jwt.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(user.Id, rawRefresh, DateTime.UtcNow.AddDays(7));

        await _uow.RefreshTokens.AddAsync(refreshToken, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(user.Id, "User", user.Id, "Register", $"New user registered: {user.Email}", ct);

        return new AuthResponse(accessToken, rawRefresh, _jwt.AccessTokenExpiresInSeconds, user.ToInfoDto());
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!token.IsValid())
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");

        var user = await _uow.Users.GetByIdAsync(token.UserId, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!user.IsActive || user.IsCurrentlyLocked())
            throw new UnauthorizedAccessException("Account is inactive or locked.");

        token.Revoke();
        _uow.RefreshTokens.Update(token);

        var newAccessToken = _jwt.GenerateAccessToken(user);
        var newRawRefresh = _jwt.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(user.Id, newRawRefresh, DateTime.UtcNow.AddDays(7));

        user.UpdateLastActivity();
        _uow.Users.Update(user);
        await _uow.RefreshTokens.AddAsync(newRefreshToken, ct);
        await _uow.SaveChangesAsync(ct);

        return new AuthResponse(newAccessToken, newRawRefresh, _jwt.AccessTokenExpiresInSeconds, user.ToInfoDto());
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, ct);
        if (token != null)
        {
            token.Revoke();
            _uow.RefreshTokens.Update(token);
            await _uow.SaveChangesAsync(ct);
        }
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Current password is incorrect.");

        var newHash = _hasher.HashPassword(user, request.NewPassword);
        user.SetPasswordHash(newHash);
        _uow.Users.Update(user);

        // Revoke all refresh tokens on password change
        await _uow.RefreshTokens.RevokeAllForUserAsync(userId, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "User", userId, "ChangePassword", "Password changed.", ct);
    }
}
