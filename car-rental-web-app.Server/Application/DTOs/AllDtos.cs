using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Entities;

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
    // Client data (creates or finds existing)
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

namespace CarRental.Application.DTOs.Branches;

public record BranchDto(
    int Id,
    string Name,
    string City,
    string Address,
    string Phone,
    int? ManagerId,
    string? ManagerName,
    bool IsActive,
    int VehicleCount,
    int ActiveRentalCount,
    DateTime CreatedAt
);

public record CreateBranchRequest(
    [Required, MinLength(2), MaxLength(100)] string Name,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(300)] string Address,
    [Required, MaxLength(20)] string Phone
);

public record UpdateBranchRequest(
    [Required, MinLength(2), MaxLength(100)] string Name,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(300)] string Address,
    [Required, MaxLength(20)] string Phone,
    bool IsActive
);

public record AssignManagerRequest([Required] int ManagerId);

namespace CarRental.Application.DTOs.Reports;

public record RevenueSummaryDto(
    decimal TotalRevenue,
    int TotalRentals,
    int ActiveRentals,
    int CompletedRentals,
    int CancelledRentals,
    decimal AverageDailyRate,
    Dictionary<string, decimal> RevenueByBranch,
    Dictionary<string, int> RentalsByCategory
);

public record DashboardStatsDto(
    int TotalVehicles,
    int AvailableVehicles,
    int RentedVehicles,
    int TotalClients,
    int ActiveRentals,
    int CompletedToday,
    decimal TodayRevenue,
    int UnreadAlerts
);

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
