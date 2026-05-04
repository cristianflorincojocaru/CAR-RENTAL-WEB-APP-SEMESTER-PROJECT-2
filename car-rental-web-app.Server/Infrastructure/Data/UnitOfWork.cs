using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Repositories;

namespace CarRental.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    private IUserRepository? _users;
    private IVehicleRepository? _vehicles;
    private IClientRepository? _clients;
    private IRentalRepository? _rentals;
    private IBranchRepository? _branches;
    private IRefreshTokenRepository? _refreshTokens;
    private IAuditLogRepository? _auditLogs;
    private ISecurityAlertRepository? _securityAlerts;

    public UnitOfWork(AppDbContext db) => _db = db;

    public IUserRepository Users => _users ??= new UserRepository(_db);
    public IVehicleRepository Vehicles => _vehicles ??= new VehicleRepository(_db);
    public IClientRepository Clients => _clients ??= new ClientRepository(_db);
    public IRentalRepository Rentals => _rentals ??= new RentalRepository(_db);
    public IBranchRepository Branches => _branches ??= new BranchRepository(_db);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_db);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_db);
    public ISecurityAlertRepository SecurityAlerts => _securityAlerts ??= new SecurityAlertRepository(_db);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _db.SaveChangesAsync(cancellationToken);

    public void Dispose() => _db.Dispose();
}
