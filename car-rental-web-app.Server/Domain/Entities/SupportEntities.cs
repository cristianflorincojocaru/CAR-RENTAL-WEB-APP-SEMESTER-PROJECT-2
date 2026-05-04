namespace CarRental.Domain.Entities;

public class RefreshToken
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User User { get; private set; } = null!;

    protected RefreshToken() { }

    public static RefreshToken Create(int userId, string token, DateTime expiresAt)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid() => !IsRevoked && !IsExpired();
}

public class AuditLog
{
    public int Id { get; private set; }
    public int? UserId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public int EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public User? User { get; private set; }

    protected AuditLog() { }

    public static AuditLog Create(int? userId, string entityType, int entityId,
        string action, string details)
    {
        return new AuditLog
        {
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
    }
}

public class SecurityAlert
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public AlertType AlertType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User User { get; private set; } = null!;

    protected SecurityAlert() { }

    public static SecurityAlert Create(int userId, AlertType alertType, string description)
    {
        return new SecurityAlert
        {
            UserId = userId,
            AlertType = alertType,
            Description = description,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead() => IsRead = true;
}
