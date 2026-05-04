using CarRental.Application.DTOs.Auth;
using CarRental.Application.DTOs.Branches;
using CarRental.Application.DTOs.Clients;
using CarRental.Application.DTOs.Rentals;
using CarRental.Application.DTOs.Vehicles;
using CarRental.Domain.Entities;

namespace CarRental.Application.Mappings;

public static class MappingExtensions
{
    // ── Vehicle ────────────────────────────────────────────────

    public static VehicleDto ToDto(this Vehicle v)
    {
        var specs = new List<VehicleSpecDto>();
        if (!string.IsNullOrEmpty(v.Transmission))
            specs.Add(new VehicleSpecDto("⚡", v.Transmission));
        if (v.Seats.HasValue)
            specs.Add(new VehicleSpecDto("👥", $"{v.Seats} seats"));
        if (!string.IsNullOrEmpty(v.FuelType))
        {
            var icon = v.FuelType.ToLower() switch
            {
                "hybrid" => "🌱",
                "electric" => "🔋",
                _ => "❄️"
            };
            specs.Add(new VehicleSpecDto(icon, v.FuelType));
        }

        return new VehicleDto(
            Id: v.Id,
            Name: v.FullName,
            Brand: v.Brand,
            Model: v.Model,
            Year: v.Year,
            FuelType: v.FuelType,
            Category: v.Category.ToString(),
            Branch: v.Branch?.Name ?? string.Empty,
            BranchId: v.BranchId,
            RegistrationNumber: v.RegistrationNumber,
            DailyRate: v.DailyRate,
            Status: v.Status.ToString(),
            Rating: v.Rating,
            ColorHex: v.ColorHex,
            Transmission: v.Transmission,
            Seats: v.Seats,
            IsOffer: v.IsOffer,
            DiscountPercent: v.DiscountPercent,
            IsActive: v.IsActive,
            Specs: specs
        );
    }

    // ── Client ─────────────────────────────────────────────────

    public static ClientDto ToDto(this Client c) => new(
        c.Id, c.FullName, c.Email, c.Phone, c.Address,
        c.IsFlagged, c.FlagReason, c.IsActive, c.CreatedAt
    );

    // ── Rental ─────────────────────────────────────────────────

    public static RentalListItemDto ToListDto(this Rental r) => new(
        r.Id,
        r.BookingReference,
        r.Vehicle?.FullName ?? string.Empty,
        r.Vehicle?.Category.ToString() ?? string.Empty,
        r.Client?.FullName ?? string.Empty,
        r.Client?.Email ?? string.Empty,
        r.StartDate,
        r.EndDate,
        r.TotalCost,
        r.Status.ToString(),
        r.PickupLocation,
        r.ReturnLocation,
        r.CreatedAt
    );

    public static RentalDto ToDto(this Rental r) => new(
        r.Id,
        r.BookingReference,
        r.VehicleId,
        r.Vehicle?.FullName ?? string.Empty,
        r.Vehicle?.Category.ToString() ?? string.Empty,
        r.ClientId,
        r.Client?.FullName ?? string.Empty,
        r.Client?.Email ?? string.Empty,
        r.BranchId,
        r.Branch?.Name ?? string.Empty,
        r.StartDate,
        r.EndDate,
        r.PickupLocation,
        r.ReturnLocation,
        r.TotalCost,
        r.Status.ToString(),
        r.CancellationReason,
        r.ProtectionPlan,
        r.Extras,
        r.Notes,
        r.PayNow,
        r.CreatedAt,
        r.CreatedByUser?.FullName ?? string.Empty,
        r.CompletedByUser?.FullName
    );

    // ── Branch ─────────────────────────────────────────────────

    public static BranchDto ToDto(this Branch b, int vehicleCount = 0, int activeRentalCount = 0) => new(
        b.Id,
        b.Name,
        b.City,
        b.Address,
        b.Phone,
        b.ManagerId,
        b.Manager?.FullName,
        b.IsActive,
        vehicleCount,
        activeRentalCount,
        b.CreatedAt
    );

    // ── User ───────────────────────────────────────────────────

    public static UserListItemDto ToListDto(this User u) => new(
        u.Id,
        u.FullName,
        u.Username,
        u.Email,
        u.Phone,
        u.Role.ToString(),
        u.BranchId,
        u.Branch?.Name,
        u.IsActive,
        u.IsCurrentlyLocked(),
        u.LastLoginAt,
        u.CreatedAt
    );

    public static UserInfoDto ToInfoDto(this User u) => new(
        u.Id,
        u.FullName,
        u.Username,
        u.Email,
        u.Phone,
        u.Role.ToString(),
        u.BranchId
    );
}
