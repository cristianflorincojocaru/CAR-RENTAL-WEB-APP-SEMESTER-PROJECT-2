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

public class ContactMessage
{
    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    protected ContactMessage() { }

    public static ContactMessage Create(string firstName, string lastName,
        string email, string? phone, string subject, string message)
    {
        return new ContactMessage
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Subject = subject,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead() => IsRead = true;
}


public class PromoCode
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public PromoType Type { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }
    // Condiții opționale
    public string? ApplicableCategory { get; private set; }  // ex: "Premium"
    public int? ApplicableVehicleId { get; private set; }    // ex: doar Duster
    public bool WeekendOnly { get; private set; }

    protected PromoCode() { }

    public static PromoCode Create(string code, PromoType type, decimal discountPercent,
        DateTime expiresAt, string? description = null, string? applicableCategory = null,
        int? applicableVehicleId = null, bool weekendOnly = false)
    {
        return new PromoCode
        {
            Code = code.ToUpper().Trim(),
            Type = type,
            DiscountPercent = discountPercent,
            ExpiresAt = expiresAt,
            IsActive = true,
            Description = description,
            ApplicableCategory = applicableCategory,
            ApplicableVehicleId = applicableVehicleId,
            WeekendOnly = weekendOnly
        };
    }

    public bool IsValid() => IsActive && DateTime.UtcNow < ExpiresAt;
}

