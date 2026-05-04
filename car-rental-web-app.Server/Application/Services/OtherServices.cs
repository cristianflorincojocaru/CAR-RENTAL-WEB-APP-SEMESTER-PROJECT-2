using CarRental.Application.DTOs.Auth;
using CarRental.Application.DTOs.Branches;
using CarRental.Application.DTOs.Reports;
using CarRental.Application.Interfaces;
using CarRental.Application.Mappings;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CarRental.Application.Services;

// ── Branch Service ─────────────────────────────────────────────────────────

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public BranchService(IUnitOfWork uow, IAuditService audit)
    {
        _uow = uow;
        _audit = audit;
    }

    public async Task<IEnumerable<BranchDto>> GetAllAsync(CancellationToken ct = default)
    {
        var branches = await _uow.Branches.GetActiveAsync(ct);
        var result = new List<BranchDto>();
        foreach (var b in branches)
        {
            var vehicles = await _uow.Vehicles.GetByBranchAsync(b.Id, ct);
            var rentals = await _uow.Rentals.GetByBranchAsync(b.Id, ct);
            result.Add(b.ToDto(
                vehicles.Count(v => v.IsActive),
                rentals.Count(r => r.Status == RentalStatus.Active)));
        }
        return result;
    }

    public async Task<BranchDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var branch = await _uow.Branches.GetByIdWithDetailsAsync(id, ct);
        if (branch == null) return null;
        var vehicles = await _uow.Vehicles.GetByBranchAsync(id, ct);
        var rentals = await _uow.Rentals.GetByBranchAsync(id, ct);
        return branch.ToDto(
            vehicles.Count(v => v.IsActive),
            rentals.Count(r => r.Status == RentalStatus.Active));
    }

    public async Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken ct = default)
    {
        if (await _uow.Branches.NameExistsAsync(request.Name, null, ct))
            throw new InvalidOperationException($"Branch '{request.Name}' already exists.");

        var branch = Branch.Create(request.Name, request.City, request.Address, request.Phone);
        await _uow.Branches.AddAsync(branch, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(null, "Branch", branch.Id, "BranchAdded",
            $"Branch '{branch.Name}' in {branch.City} created.", ct);

        return branch.ToDto();
    }

    public async Task<BranchDto> UpdateAsync(int id, UpdateBranchRequest request, CancellationToken ct = default)
    {
        var branch = await _uow.Branches.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Branch {id} not found.");

        if (await _uow.Branches.NameExistsAsync(request.Name, id, ct))
            throw new InvalidOperationException($"Branch name '{request.Name}' already in use.");

        branch.SetName(request.Name);
        branch.SetCity(request.City);
        branch.SetAddress(request.Address);
        branch.SetPhone(request.Phone);
        branch.SetIsActive(request.IsActive);

        _uow.Branches.Update(branch);
        await _uow.SaveChangesAsync(ct);

        return branch.ToDto();
    }

    public async Task AssignManagerAsync(int branchId, int managerId, int adminId, CancellationToken ct = default)
    {
        var branch = await _uow.Branches.GetByIdAsync(branchId, ct)
            ?? throw new KeyNotFoundException($"Branch {branchId} not found.");

        var manager = await _uow.Users.GetByIdAsync(managerId, ct)
            ?? throw new KeyNotFoundException($"User {managerId} not found.");

        if (manager.Role != UserRole.Manager)
            throw new InvalidOperationException("Only users with role 'Manager' can be assigned as branch managers.");

        branch.AssignManager(managerId);
        _uow.Branches.Update(branch);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(adminId, "Branch", branchId, "ManagerAssigned",
            $"User {manager.FullName} assigned as manager of branch '{branch.Name}'.", ct);
    }

    public async Task DeactivateAsync(int id, int adminId, CancellationToken ct = default)
    {
        var branch = await _uow.Branches.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Branch {id} not found.");

        var activeRentals = await _uow.Rentals.GetByBranchAsync(id, ct);
        if (activeRentals.Any(r => r.Status == RentalStatus.Active))
            throw new InvalidOperationException("Cannot deactivate a branch with active rentals.");

        branch.Deactivate();
        _uow.Branches.Update(branch);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(adminId, "Branch", id, "BranchDeactivated",
            $"Branch '{branch.Name}' deactivated.", ct);
    }
}

// ── User Service ───────────────────────────────────────────────────────────

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(IUnitOfWork uow, IAuditService audit, IPasswordHasher<User> hasher)
    {
        _uow = uow;
        _audit = audit;
        _hasher = hasher;
    }

    public async Task<IEnumerable<UserListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _uow.Users.GetAllWithBranchAsync(ct);
        return users.Select(u => u.ToListDto());
    }

    public async Task<UserListItemDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdWithDetailsAsync(id, ct);
        return user?.ToListDto();
    }

    public async Task<UserListItemDto> CreateAsync(CreateUserRequest request, int createdByUserId, CancellationToken ct = default)
    {
        if (await _uow.Users.EmailExistsAsync(request.Email, ct))
            throw new InvalidOperationException($"Email '{request.Email}' already registered.");

        if (await _uow.Users.UsernameExistsAsync(request.Username, ct))
            throw new InvalidOperationException($"Username '{request.Username}' already taken.");

        // Generate temporary password
        var tempPassword = $"TempPass{Guid.NewGuid().ToString("N")[..8]}!1";
        var tempUser = User.Create(request.Username, "", request.Email, request.FullName,
            request.Phone, request.Role, request.BranchId, createdByUserId);
        var hash = _hasher.HashPassword(tempUser, tempPassword);

        var user = User.Create(request.Username, hash, request.Email, request.FullName,
            request.Phone, request.Role, request.BranchId, createdByUserId);

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(createdByUserId, "User", user.Id, "StaffAdded",
            $"Staff member {user.FullName} ({user.Role}) created by user {createdByUserId}.", ct);

        var created = await _uow.Users.GetByIdWithDetailsAsync(user.Id, ct);
        return created!.ToListDto();
    }

    public async Task<UserListItemDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        user.SetFullName(request.FullName);
        user.SetPhone(request.Phone);
        user.SetRole(request.Role);
        user.SetBranchId(request.BranchId);
        user.SetIsActive(request.IsActive);

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        var updated = await _uow.Users.GetByIdWithDetailsAsync(id, ct);
        return updated!.ToListDto();
    }

    public async Task LockAsync(int id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"User {id} not found.");
        user.LockAccount();
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task UnlockAsync(int id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"User {id} not found.");
        user.UnlockAccount();
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync(null, "User", id, "AccountUnlocked", $"Account for user {user.FullName} unlocked.", ct);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"User {id} not found.");
        user.SetIsActive(false);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync(null, "User", id, "UserDeactivated", $"User {user.FullName} deactivated.", ct);
    }
}

// ── Report Service ─────────────────────────────────────────────────────────

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow) => _uow = uow;

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime? from, DateTime? to,
        int? branchId, CancellationToken ct = default)
    {
        var rentals = await _uow.Rentals.GetByFiltersAsync(branchId, null, from, to, ct);
        var rentalsList = rentals.ToList();

        var branches = await _uow.Branches.GetActiveAsync(ct);
        var revenueByBranch = branches.ToDictionary(
            b => b.Name,
            b => rentalsList.Where(r => r.BranchId == b.Id && r.Status == RentalStatus.Completed)
                           .Sum(r => r.TotalCost));

        var vehicles = await _uow.Vehicles.GetAllAsync(ct);
        var rentalsByCategory = Enum.GetValues<VehicleCategory>().ToDictionary(
            cat => cat.ToString(),
            cat =>
            {
                var vehicleIds = vehicles.Where(v => v.Category == cat).Select(v => v.Id).ToHashSet();
                return rentalsList.Count(r => vehicleIds.Contains(r.VehicleId));
            });

        var completed = rentalsList.Where(r => r.Status == RentalStatus.Completed).ToList();
        return new RevenueSummaryDto(
            TotalRevenue: completed.Sum(r => r.TotalCost),
            TotalRentals: rentalsList.Count,
            ActiveRentals: rentalsList.Count(r => r.Status == RentalStatus.Active),
            CompletedRentals: completed.Count,
            CancelledRentals: rentalsList.Count(r => r.Status == RentalStatus.Cancelled),
            AverageDailyRate: completed.Count > 0
                ? completed.Average(r => r.TotalCost / Math.Max(r.DurationDays, 1))
                : 0,
            RevenueByBranch: revenueByBranch,
            RentalsByCategory: rentalsByCategory
        );
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(int? branchId, CancellationToken ct = default)
    {
        var allVehicles = branchId.HasValue
            ? await _uow.Vehicles.GetByBranchAsync(branchId.Value, ct)
            : await _uow.Vehicles.GetAllAsync(ct);

        var activeVehicles = allVehicles.Where(v => v.IsActive).ToList();
        var allClients = await _uow.Clients.GetAllAsync(ct);
        var allRentals = await _uow.Rentals.GetByFiltersAsync(branchId, null, null, null, ct);
        var rentalsList = allRentals.ToList();

        var today = DateTime.UtcNow.Date;
        var completedToday = rentalsList.Count(r =>
            r.Status == RentalStatus.Completed && r.UpdatedAt.Date == today);
        var todayRevenue = rentalsList
            .Where(r => r.Status == RentalStatus.Completed && r.UpdatedAt.Date == today)
            .Sum(r => r.TotalCost);

        var alertCount = await _uow.SecurityAlerts.GetUnreadCountAsync(ct);

        return new DashboardStatsDto(
            TotalVehicles: activeVehicles.Count,
            AvailableVehicles: activeVehicles.Count(v => v.Status == VehicleStatus.Available),
            RentedVehicles: activeVehicles.Count(v => v.Status == VehicleStatus.Rented),
            TotalClients: allClients.Count(),
            ActiveRentals: rentalsList.Count(r => r.Status == RentalStatus.Active),
            CompletedToday: completedToday,
            TodayRevenue: todayRevenue,
            UnreadAlerts: alertCount
        );
    }
}

// ── Audit Service ──────────────────────────────────────────────────────────

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _uow;

    public AuditService(IUnitOfWork uow) => _uow = uow;

    public async Task LogAsync(int? userId, string entityType, int entityId,
        string action, string details, CancellationToken ct = default)
    {
        var log = AuditLog.Create(userId, entityType, entityId, action, details);
        await _uow.AuditLogs.AddAsync(log, ct);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Security Alert Service ─────────────────────────────────────────────────

public class SecurityAlertService : ISecurityAlertService
{
    private readonly IUnitOfWork _uow;

    public SecurityAlertService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<object>> GetUnreadAlertsAsync(CancellationToken ct = default)
    {
        var alerts = await _uow.SecurityAlerts.GetUnreadAsync(ct);
        return alerts.Select(a => new
        {
            a.Id,
            a.UserId,
            UserName = a.User?.FullName,
            AlertType = a.AlertType.ToString(),
            a.Description,
            a.IsRead,
            a.CreatedAt
        });
    }

    public async Task MarkAsReadAsync(int alertId, CancellationToken ct = default)
    {
        var alert = await _uow.SecurityAlerts.GetByIdAsync(alertId, ct)
            ?? throw new KeyNotFoundException($"Alert {alertId} not found.");
        alert.MarkAsRead();
        _uow.SecurityAlerts.Update(alert);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default)
        => await _uow.SecurityAlerts.GetUnreadCountAsync(ct);
}
