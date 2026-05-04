using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IVehicleRepository Vehicles { get; }
    IClientRepository Clients { get; }
    IRentalRepository Rentals { get; }
    IBranchRepository Branches { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IAuditLogRepository AuditLogs { get; }
    ISecurityAlertRepository SecurityAlerts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    Task<IEnumerable<User>> GetByBranchAsync(int branchId, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllWithBranchAsync(CancellationToken ct = default);
    Task<User?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
}

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> GetAvailableAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<IEnumerable<Vehicle>> GetByBranchAsync(int branchId, CancellationToken ct = default);
    Task<IEnumerable<Vehicle>> GetByFiltersAsync(int? branchId, VehicleCategory? category,
        DateTime? from, DateTime? to, string? transmission, bool? isOffer, CancellationToken ct = default);
    Task<bool> RegistrationExistsAsync(string registrationNumber, int? excludeId = null, CancellationToken ct = default);
    Task<Vehicle?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> HasOverlapAsync(int vehicleId, DateTime from, DateTime to, int? excludeRentalId = null, CancellationToken ct = default);
}

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default);
    Task<IEnumerable<Client>> SearchAsync(string term, CancellationToken ct = default);
    Task<Client?> GetByIdWithRentalsAsync(int id, CancellationToken ct = default);
}

public interface IRentalRepository : IRepository<Rental>
{
    Task<IEnumerable<Rental>> GetByClientAsync(int clientId, CancellationToken ct = default);
    Task<IEnumerable<Rental>> GetByVehicleAsync(int vehicleId, CancellationToken ct = default);
    Task<IEnumerable<Rental>> GetByBranchAsync(int branchId, CancellationToken ct = default);
    Task<IEnumerable<Rental>> GetActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<Rental>> GetByFiltersAsync(int? branchId, RentalStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<Rental?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<Rental?> GetByReferenceAsync(string reference, CancellationToken ct = default);
    Task<IEnumerable<Rental>> GetCompletedByPeriodAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public interface IBranchRepository : IRepository<Branch>
{
    Task<IEnumerable<Branch>> GetActiveAsync(CancellationToken ct = default);
    Task<Branch?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
}

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(int userId, CancellationToken ct = default);
}

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken ct = default);
    Task<IEnumerable<AuditLog>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50, CancellationToken ct = default);
}

public interface ISecurityAlertRepository : IRepository<SecurityAlert>
{
    Task<IEnumerable<SecurityAlert>> GetUnreadAsync(CancellationToken ct = default);
    Task<IEnumerable<SecurityAlert>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);
}
