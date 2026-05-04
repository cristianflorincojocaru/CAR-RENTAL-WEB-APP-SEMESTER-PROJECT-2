using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Entities;

namespace CarRental.Application.DTOs.Vehicles;

public record VehicleDto(
    int Id,
    string Name,
    string Brand,
    string Model,
    int Year,
    string? FuelType,
    string Category,
    string Branch,
    int BranchId,
    string RegistrationNumber,
    decimal DailyRate,
    string Status,
    decimal? Rating,
    string? ColorHex,
    string? Transmission,
    int? Seats,
    bool IsOffer,
    int? DiscountPercent,
    bool IsActive,
    List<VehicleSpecDto> Specs
);

public record VehicleSpecDto(string Icon, string Value);

public record CreateVehicleRequest(
    [Required, MaxLength(20)] string RegistrationNumber,
    [Required, MaxLength(100)] string Brand,
    [Required, MaxLength(100)] string Model,
    [Range(1990, 2030)] int Year,
    VehicleCategory Category,
    [Range(1, 10000)] decimal DailyRate,
    string? FuelType,
    string? Transmission,
    [Range(1, 9)] int? Seats,
    string? ColorHex,
    int BranchId
);

public record UpdateVehicleRequest(
    [Required, MaxLength(20)] string RegistrationNumber,
    [Required, MaxLength(100)] string Brand,
    [Required, MaxLength(100)] string Model,
    [Range(1990, 2030)] int Year,
    VehicleCategory Category,
    [Range(1, 10000)] decimal DailyRate,
    string? FuelType,
    string? Transmission,
    [Range(1, 9)] int? Seats,
    string? ColorHex,
    int BranchId,
    bool IsActive
);

public record SetOfferRequest(
    bool IsOffer,
    [Range(1, 90)] int? DiscountPercent
);

public record VehicleFilterRequest(
    string? Branch,
    string? Category,
    DateTime? PickupDate,
    DateTime? ReturnDate,
    string? Transmission,
    bool? IsOffer
);
