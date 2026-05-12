using CarRental.Application.DTOs.Auth;
using CarRental.Application.DTOs.Branches;
using CarRental.Application.DTOs.Clients;
using CarRental.Application.DTOs.Rentals;
using CarRental.Application.DTOs.Reports;
using CarRental.Application.DTOs.Vehicles;
using CarRental.Domain.Entities;

namespace CarRental.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default);
}

public interface IUserService
{
    Task<IEnumerable<UserListItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserListItemDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserListItemDto> CreateAsync(CreateUserRequest request, int createdByUserId, CancellationToken ct = default);
    Task<UserListItemDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task LockAsync(int id, CancellationToken ct = default);
    Task UnlockAsync(int id, CancellationToken ct = default);
    Task DeactivateAsync(int id, CancellationToken ct = default);
}

public interface IVehicleService
{
    Task<IEnumerable<VehicleDto>> GetAllAsync(VehicleFilterRequest filter, CancellationToken ct = default);
    Task<VehicleDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<VehicleDto> CreateAsync(CreateVehicleRequest request, int addedByUserId, CancellationToken ct = default);
    Task<VehicleDto> UpdateAsync(int id, UpdateVehicleRequest request, CancellationToken ct = default);
    Task DeactivateAsync(int id, int userId, CancellationToken ct = default);
    Task SetOfferAsync(int id, SetOfferRequest request, int userId, CancellationToken ct = default);
    Task<IEnumerable<string>> GetBranchNamesAsync(CancellationToken ct = default);
}

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllAsync(string? search = null, CancellationToken ct = default);
    Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default);
    Task<ClientDto> UpdateAsync(int id, UpdateClientRequest request, CancellationToken ct = default);
    Task FlagAsync(int id, FlagClientRequest request, int userId, CancellationToken ct = default);
    Task UnflagAsync(int id, int userId, CancellationToken ct = default);
    Task<IEnumerable<RentalListItemDto>> GetRentalHistoryAsync(int clientId, CancellationToken ct = default);
}

public interface IRentalService
{
    Task<IEnumerable<RentalListItemDto>> GetAllAsync(int? branchId, RentalStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<RentalDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RentalDto> CreateAsync(CreateRentalRequest request, int createdByUserId, CancellationToken ct = default);
    Task<RentalDto> CompleteAsync(int id, int completedByUserId, CancellationToken ct = default);
    Task<RentalDto> CancelAsync(int id, string reason, int userId, CancellationToken ct = default);
    Task<IEnumerable<RentalListItemDto>> GetByClientAsync(int clientId, CancellationToken ct = default);

    Task<IEnumerable<RentalListItemDto>> GetByClientEmailAsync(string email, CancellationToken ct = default);
}

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllAsync(CancellationToken ct = default);
    Task<BranchDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken ct = default);
    Task<BranchDto> UpdateAsync(int id, UpdateBranchRequest request, CancellationToken ct = default);
    Task AssignManagerAsync(int branchId, int managerId, int adminId, CancellationToken ct = default);
    Task DeactivateAsync(int id, int adminId, CancellationToken ct = default);
}

public interface IReportService
{
    Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default);
    Task<DashboardStatsDto> GetDashboardStatsAsync(int? branchId, CancellationToken ct = default);
}

public interface IAuditService
{
    Task LogAsync(int? userId, string entityType, int entityId, string action, string details, CancellationToken ct = default);
}

public interface ISecurityAlertService
{
    Task<IEnumerable<object>> GetUnreadAlertsAsync(CancellationToken ct = default);
    Task MarkAsReadAsync(int alertId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);
}

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    int AccessTokenExpiresInSeconds { get; }
}
