using System.ComponentModel.DataAnnotations;

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
