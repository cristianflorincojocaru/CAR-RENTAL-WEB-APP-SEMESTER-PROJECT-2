namespace CarRental.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public UserRole Role { get; private set; }
    public int? BranchId { get; private set; }
    public int? CreatedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsLocked { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Branch? Branch { get; private set; }
    public User? CreatedByUser { get; private set; }
    public ICollection<Rental> CreatedRentals { get; private set; } = new List<Rental>();
    public ICollection<Rental> CompletedRentals { get; private set; } = new List<Rental>();
    public ICollection<Vehicle> AddedVehicles { get; private set; } = new List<Vehicle>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<AuditLog> AuditLogs { get; private set; } = new List<AuditLog>();
    public ICollection<SecurityAlert> SecurityAlerts { get; private set; } = new List<SecurityAlert>();

    protected User() { }

    public static User Create(string username, string passwordHash, string email,
        string fullName, string? phone, UserRole role, int? branchId = null, int? createdByUserId = null)
    {
        return new User
        {
            Username = username.ToLower().Trim(),
            PasswordHash = passwordHash,
            Email = email.ToLower().Trim(),
            FullName = fullName.Trim(),
            Phone = phone?.Trim(),
            Role = role,
            BranchId = branchId,
            CreatedByUserId = createdByUserId,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetPasswordHash(string hash) => PasswordHash = hash;
    public void SetEmail(string email) => Email = email.ToLower().Trim();
    public void SetFullName(string name) => FullName = name.Trim();
    public void SetPhone(string? phone) => Phone = phone?.Trim();
    public void SetRole(UserRole role) => Role = role;
    public void SetBranchId(int? branchId) => BranchId = branchId;
    public void SetIsActive(bool isActive) => IsActive = isActive;

    public void RecordLogin()
    {
        FailedLoginAttempts = 0;
        IsLocked = false;
        LockedUntil = null;
        LastLoginAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            IsLocked = true;
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }
    }

    public void UpdateLastActivity() => LastActivityAt = DateTime.UtcNow;

    public void LockAccount()
    {
        IsLocked = true;
        LockedUntil = DateTime.UtcNow.AddHours(24);
    }

    public void UnlockAccount()
    {
        IsLocked = false;
        LockedUntil = null;
        FailedLoginAttempts = 0;
    }

    public bool IsCurrentlyLocked() =>
        IsLocked && (LockedUntil == null || LockedUntil > DateTime.UtcNow);
}
