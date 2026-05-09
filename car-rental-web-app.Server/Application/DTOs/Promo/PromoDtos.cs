using System.ComponentModel.DataAnnotations;

namespace CarRental.Application.DTOs.Promo;

public record ValidatePromoRequest(
    [Required] string Code,
    [Required] int VehicleId,
    [Required] string PickupDate,
    [Required] string ReturnDate
);

public record PromoValidationResult(
    bool IsValid,
    string? ErrorMessage,
    string? Code,
    decimal? DiscountPercent,
    string? Description
);