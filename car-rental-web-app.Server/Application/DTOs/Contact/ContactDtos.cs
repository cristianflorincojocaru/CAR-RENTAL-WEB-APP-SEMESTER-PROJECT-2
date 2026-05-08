using System.ComponentModel.DataAnnotations;

namespace CarRental.Application.DTOs.Contact;

public record ContactMessageRequest(
    [Required, MinLength(2)] string FirstName,
    [Required, MinLength(2)] string LastName,
    [Required, EmailAddress] string Email,
    string? Phone,
    [Required] string Subject,
    [Required, MinLength(10)] string Message
);

public record ContactMessageResponse(bool Success, string Message);
