using System.ComponentModel.DataAnnotations;

namespace CarRental.Application.DTOs.Rentals;

public record RentalDto(
    int Id,
    string BookingReference,
    int VehicleId,
    string VehicleName,
    string VehicleCategory,
    int ClientId,
    string ClientName,
    string ClientEmail,
    int BranchId,
    string BranchName,
    DateTime StartDate,
    DateTime EndDate,
    string PickupLocation,
    string ReturnLocation,
    decimal TotalCost,
    string Status,
    string? CancellationReason,
    string? ProtectionPlan,
    string? Extras,
    string? Notes,
    bool PayNow,
    DateTime CreatedAt,
    string CreatedByUserName,
    string? CompletedByUserName
);

public record CreateRentalRequest(
    [Required] int VehicleId,
    [Required] DateTime PickupDate,
    [Required] DateTime ReturnDate,
    [Required, MaxLength(200)] string PickupLocation,
    [Required, MaxLength(200)] string ReturnLocation,
    [Required, MaxLength(50)] string ProtectionPlan,
    List<string>? Extras,
    [Required, MinLength(2)] string FirstName,
    [Required, MinLength(2)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    string? FlightNumber,
    string? Notes,
    bool PayNow = true
);

public record CompleteRentalRequest(int CompletedByUserId);

public record CancelRentalRequest([Required, MinLength(3)] string Reason);

public record RentalListItemDto(
    int Id,
    string BookingReference,
    string VehicleName,
    string VehicleCategory,
    string ClientName,
    string ClientEmail,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalCost,
    string Status,
    string PickupLocation,
    string ReturnLocation,
    DateTime CreatedAt
);
