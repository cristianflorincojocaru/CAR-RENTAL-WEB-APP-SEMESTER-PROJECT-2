namespace CarRental.Domain.Entities;

public class Branch
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public int? ManagerId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public User? Manager { get; private set; }
    public ICollection<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();
    public ICollection<User> Staff { get; private set; } = new List<User>();
    public ICollection<Rental> Rentals { get; private set; } = new List<Rental>();

    protected Branch() { }

    public static Branch Create(string name, string city, string address, string phone)
    {
        return new Branch
        {
            Name = name.Trim(),
            City = city.Trim(),
            Address = address.Trim(),
            Phone = phone.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetName(string name) { Name = name.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetCity(string city) { City = city.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetAddress(string address) { Address = address.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetPhone(string phone) { Phone = phone.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetManagerId(int? managerId) { ManagerId = managerId; UpdatedAt = DateTime.UtcNow; }
    public void SetIsActive(bool isActive) { IsActive = isActive; UpdatedAt = DateTime.UtcNow; }
    public void AssignManager(int userId) { ManagerId = userId; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
