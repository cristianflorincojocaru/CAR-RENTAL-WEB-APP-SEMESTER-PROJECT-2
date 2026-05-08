using System.ComponentModel.DataAnnotations;

namespace CarRental.Application.DTOs.Clients;

public record ClientDto(
    int Id,
    string FullName,
    string Email,
    string Phone,
    string? Address,
    bool IsFlagged,
    string? FlagReason,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateClientRequest(
    [Required, MinLength(2), MaxLength(150)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MaxLength(20)] string Phone,
    [MaxLength(300)] string? Address
);

public record UpdateClientRequest(
    [Required, MinLength(2), MaxLength(150)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MaxLength(20)] string Phone,
    [MaxLength(300)] string? Address,
    bool IsActive
);

public record FlagClientRequest([Required, MinLength(3)] string Reason);
