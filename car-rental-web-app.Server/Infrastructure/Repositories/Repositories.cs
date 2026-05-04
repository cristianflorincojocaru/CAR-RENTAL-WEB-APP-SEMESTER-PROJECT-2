using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories;

// ── Generic ────────────────────────────────────────────────────────────────

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _set.FindAsync(new object[] { id }, ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
}

// ── User Repository ────────────────────────────────────────────────────────

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim(), ct);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(u => u.Username == username.ToLower().Trim(), ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _set.AnyAsync(u => u.Email == email.ToLower().Trim(), ct);

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => await _set.AnyAsync(u => u.Username == username.ToLower().Trim(), ct);

    public async Task<IEnumerable<User>> GetByBranchAsync(int branchId, CancellationToken ct = default)
        => await _set.Where(u => u.BranchId == branchId).ToListAsync(ct);

    public async Task<IEnumerable<User>> GetAllWithBranchAsync(CancellationToken ct = default)
        => await _set.Include(u => u.Branch).ToListAsync(ct);

    public async Task<User?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _set.Include(u => u.Branch).FirstOrDefaultAsync(u => u.Id == id, ct);
}

// ── Vehicle Repository ─────────────────────────────────────────────────────

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Vehicle>> GetAvailableAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var query = _set.Include(v => v.Branch)
            .Where(v => v.IsActive && v.Status == VehicleStatus.Available);

        if (from.HasValue && to.HasValue)
        {
            var bookedIds = await _db.Rentals
                .Where(r => r.Status == RentalStatus.Active &&
                            r.StartDate < to.Value && r.EndDate > from.Value)
                .Select(r => r.VehicleId)
                .ToListAsync(ct);

            query = query.Where(v => !bookedIds.Contains(v.Id));
        }

        return await query.ToListAsync(ct);
    }

    public async Task<IEnumerable<Vehicle>> GetByBranchAsync(int branchId, CancellationToken ct = default)
        => await _set.Include(v => v.Branch)
                     .Where(v => v.BranchId == branchId)
                     .ToListAsync(ct);

    public async Task<IEnumerable<Vehicle>> GetByFiltersAsync(int? branchId, VehicleCategory? category,
        DateTime? from, DateTime? to, string? transmission, bool? isOffer, CancellationToken ct = default)
    {
        var query = _set.Include(v => v.Branch).AsQueryable();

        if (branchId.HasValue)
            query = query.Where(v => v.BranchId == branchId.Value);

        if (category.HasValue)
            query = query.Where(v => v.Category == category.Value);

        if (!string.IsNullOrWhiteSpace(transmission))
            query = query.Where(v => v.Transmission != null &&
                v.Transmission.ToLower() == transmission.ToLower());

        if (isOffer.HasValue)
            query = query.Where(v => v.IsOffer == isOffer.Value);

        if (from.HasValue && to.HasValue)
        {
            var bookedIds = await _db.Rentals
                .Where(r => r.Status == RentalStatus.Active &&
                            r.StartDate < to.Value && r.EndDate > from.Value)
                .Select(r => r.VehicleId)
                .ToListAsync(ct);

            query = query.Where(v => !bookedIds.Contains(v.Id));
        }

        return await query.ToListAsync(ct);
    }

    public async Task<bool> RegistrationExistsAsync(string reg, int? excludeId = null, CancellationToken ct = default)
    {
        var q = _set.Where(v => v.RegistrationNumber == reg.ToUpper().Trim());
        if (excludeId.HasValue) q = q.Where(v => v.Id != excludeId.Value);
        return await q.AnyAsync(ct);
    }

    public async Task<Vehicle?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _set.Include(v => v.Branch)
                     .Include(v => v.AddedByUser)
                     .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<bool> HasOverlapAsync(int vehicleId, DateTime from, DateTime to,
        int? excludeRentalId = null, CancellationToken ct = default)
    {
        var q = _db.Rentals.Where(r =>
            r.VehicleId == vehicleId &&
            r.Status == RentalStatus.Active &&
            r.StartDate < to && r.EndDate > from);

        if (excludeRentalId.HasValue)
            q = q.Where(r => r.Id != excludeRentalId.Value);

        return await q.AnyAsync(ct);
    }
}

// ── Client Repository ──────────────────────────────────────────────────────

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(AppDbContext db) : base(db) { }

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(c => c.Email == email.ToLower().Trim(), ct);

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var q = _set.Where(c => c.Email == email.ToLower().Trim());
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync(ct);
    }

    public async Task<IEnumerable<Client>> SearchAsync(string term, CancellationToken ct = default)
    {
        term = term.ToLower().Trim();
        return await _set
            .Where(c => c.FullName.ToLower().Contains(term) ||
                        c.Email.ToLower().Contains(term) ||
                        c.Phone.Contains(term))
            .ToListAsync(ct);
    }

    public async Task<Client?> GetByIdWithRentalsAsync(int id, CancellationToken ct = default)
        => await _set.Include(c => c.Rentals)
                     .FirstOrDefaultAsync(c => c.Id == id, ct);
}

// ── Rental Repository ──────────────────────────────────────────────────────

public class RentalRepository : Repository<Rental>, IRentalRepository
{
    public RentalRepository(AppDbContext db) : base(db) { }

    private IQueryable<Rental> WithDetails() =>
        _set.Include(r => r.Vehicle).ThenInclude(v => v.Branch)
            .Include(r => r.Client)
            .Include(r => r.Branch)
            .Include(r => r.CreatedByUser)
            .Include(r => r.CompletedByUser);

    public async Task<IEnumerable<Rental>> GetByClientAsync(int clientId, CancellationToken ct = default)
        => await WithDetails().Where(r => r.ClientId == clientId).ToListAsync(ct);

    public async Task<IEnumerable<Rental>> GetByVehicleAsync(int vehicleId, CancellationToken ct = default)
        => await WithDetails().Where(r => r.VehicleId == vehicleId).ToListAsync(ct);

    public async Task<IEnumerable<Rental>> GetByBranchAsync(int branchId, CancellationToken ct = default)
        => await WithDetails().Where(r => r.BranchId == branchId).ToListAsync(ct);

    public async Task<IEnumerable<Rental>> GetActiveAsync(CancellationToken ct = default)
        => await WithDetails().Where(r => r.Status == RentalStatus.Active).ToListAsync(ct);

    public async Task<IEnumerable<Rental>> GetByFiltersAsync(int? branchId, RentalStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var q = WithDetails().AsQueryable();
        if (branchId.HasValue) q = q.Where(r => r.BranchId == branchId.Value);
        if (status.HasValue) q = q.Where(r => r.Status == status.Value);
        if (from.HasValue) q = q.Where(r => r.StartDate >= from.Value);
        if (to.HasValue) q = q.Where(r => r.EndDate <= to.Value);
        return await q.ToListAsync(ct);
    }

    public async Task<Rental?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await WithDetails().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Rental?> GetByReferenceAsync(string reference, CancellationToken ct = default)
        => await WithDetails().FirstOrDefaultAsync(r => r.BookingReference == reference, ct);

    public async Task<IEnumerable<Rental>> GetCompletedByPeriodAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await WithDetails()
            .Where(r => r.Status == RentalStatus.Completed && r.EndDate >= from && r.EndDate <= to)
            .ToListAsync(ct);
}

// ── Branch Repository ──────────────────────────────────────────────────────

public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Branch>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Include(b => b.Manager).Where(b => b.IsActive).ToListAsync(ct);

    public async Task<Branch?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _set.Include(b => b.Manager).FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var q = _set.Where(b => b.Name.ToLower() == name.ToLower().Trim());
        if (excludeId.HasValue) q = q.Where(b => b.Id != excludeId.Value);
        return await q.AnyAsync(ct);
    }
}

// ── RefreshToken Repository ────────────────────────────────────────────────

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext db) : base(db) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _set.Include(t => t.User)
                     .FirstOrDefaultAsync(t => t.Token == token, ct);

    public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
    {
        var tokens = await _set
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);
        foreach (var t in tokens) t.Revoke();
    }
}

// ── AuditLog Repository ────────────────────────────────────────────────────

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken ct = default)
        => await _set.Include(a => a.User)
                     .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                     .OrderByDescending(a => a.Timestamp)
                     .ToListAsync(ct);

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(int userId, CancellationToken ct = default)
        => await _set.Where(a => a.UserId == userId)
                     .OrderByDescending(a => a.Timestamp)
                     .ToListAsync(ct);

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50, CancellationToken ct = default)
        => await _set.Include(a => a.User)
                     .OrderByDescending(a => a.Timestamp)
                     .Take(count)
                     .ToListAsync(ct);
}

// ── SecurityAlert Repository ───────────────────────────────────────────────

public class SecurityAlertRepository : Repository<SecurityAlert>, ISecurityAlertRepository
{
    public SecurityAlertRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<SecurityAlert>> GetUnreadAsync(CancellationToken ct = default)
        => await _set.Include(s => s.User)
                     .Where(s => !s.IsRead)
                     .OrderByDescending(s => s.CreatedAt)
                     .ToListAsync(ct);

    public async Task<IEnumerable<SecurityAlert>> GetByUserAsync(int userId, CancellationToken ct = default)
        => await _set.Where(s => s.UserId == userId)
                     .OrderByDescending(s => s.CreatedAt)
                     .ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default)
        => await _set.CountAsync(s => !s.IsRead, ct);
}
