namespace CarRental.Domain.Entities;

public class Client
{
    public int Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public bool IsFlagged { get; private set; }
    public string? FlagReason { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Rental> Rentals { get; private set; } = new List<Rental>();

    protected Client() { }

    public static Client Create(string fullName, string email, string phone, string? address = null)
    {
        return new Client
        {
            FullName = fullName.Trim(),
            Email = email.ToLower().Trim(),
            Phone = phone.Trim(),
            Address = address?.Trim(),
            IsFlagged = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetFullName(string name) { FullName = name.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetEmail(string email) { Email = email.ToLower().Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetPhone(string phone) { Phone = phone.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetAddress(string? address) { Address = address?.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetIsActive(bool isActive) { IsActive = isActive; UpdatedAt = DateTime.UtcNow; }

    public void Flag(string reason)
    {
        IsFlagged = true;
        FlagReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unflag()
    {
        IsFlagged = false;
        FlagReason = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
