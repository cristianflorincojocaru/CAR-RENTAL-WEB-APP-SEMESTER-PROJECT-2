using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Entities;

namespace CarRental.Application.DTOs.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RegisterRequest(
    [Required, MinLength(2), MaxLength(100)] string FullName,
    [Required, MinLength(3), MaxLength(50), RegularExpression(@"^[a-z0-9_]+$")] string Username,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(7), MaxLength(20)] string Phone,
    [Required, MinLength(8)] string Password
);

public record RefreshTokenRequest([Required] string RefreshToken);

public record ChangePasswordRequest(
    [Required] string OldPassword,
    [Required, MinLength(8)] string NewPassword
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserInfoDto User
);

public record UserInfoDto(
    int Id,
    string FullName,
    string Username,
    string Email,
    string? Phone,
    string Role,
    int? BranchId
);

public record CreateUserRequest(
    [Required, MinLength(2), MaxLength(100)] string FullName,
    [Required, MinLength(3), MaxLength(50)] string Username,
    [Required, EmailAddress] string Email,
    string? Phone,
    [Required] UserRole Role,
    int? BranchId
);

public record UpdateUserRequest(
    [Required, MinLength(2), MaxLength(100)] string FullName,
    string? Phone,
    UserRole Role,
    int? BranchId,
    bool IsActive
);

public record UserListItemDto(
    int Id,
    string FullName,
    string Username,
    string Email,
    string? Phone,
    string Role,
    int? BranchId,
    string? BranchName,
    bool IsActive,
    bool IsLocked,
    DateTime? LastLoginAt,
    DateTime CreatedAt
);
