namespace CarRental.Domain.Entities;

public class Vehicle
{
    public int Id { get; private set; }
    public string RegistrationNumber { get; private set; } = string.Empty;
    public int BranchId { get; private set; }
    public int AddedByUserId { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public VehicleCategory Category { get; private set; }
    public decimal DailyRate { get; private set; }
    public VehicleStatus Status { get; private set; } = VehicleStatus.Available;
    public bool IsActive { get; private set; } = true;
    public bool IsOffer { get; private set; }
    public int? DiscountPercent { get; private set; }
    public string? ColorHex { get; private set; }
    public string? FuelType { get; private set; }
    public string? Transmission { get; private set; }
    public int? Seats { get; private set; }
    public decimal? Rating { get; private set; } = 4.5m;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Branch Branch { get; private set; } = null!;
    public User AddedByUser { get; private set; } = null!;
    public ICollection<Rental> Rentals { get; private set; } = new List<Rental>();

    protected Vehicle() { }

    public static Vehicle Create(string registrationNumber, int branchId, int addedByUserId,
        string brand, string model, int year, VehicleCategory category, decimal dailyRate,
        string? fuelType = null, string? transmission = null, int? seats = null,
        string? colorHex = null)
    {
        return new Vehicle
        {
            RegistrationNumber = registrationNumber.ToUpper().Trim(),
            BranchId = branchId,
            AddedByUserId = addedByUserId,
            Brand = brand.Trim(),
            Model = model.Trim(),
            Year = year,
            Category = category,
            DailyRate = dailyRate,
            FuelType = fuelType?.Trim(),
            Transmission = transmission?.Trim(),
            Seats = seats,
            ColorHex = colorHex?.Trim(),
            Status = VehicleStatus.Available,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetRegistrationNumber(string v) { RegistrationNumber = v.ToUpper().Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetBranchId(int id) { BranchId = id; UpdatedAt = DateTime.UtcNow; }
    public void SetBrand(string b) { Brand = b.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetModel(string m) { Model = m.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetYear(int y) { Year = y; UpdatedAt = DateTime.UtcNow; }
    public void SetCategory(VehicleCategory c) { Category = c; UpdatedAt = DateTime.UtcNow; }
    public void SetDailyRate(decimal r) { DailyRate = r; UpdatedAt = DateTime.UtcNow; }
    public void SetFuelType(string? f) { FuelType = f?.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetTransmission(string? t) { Transmission = t?.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetSeats(int? s) { Seats = s; UpdatedAt = DateTime.UtcNow; }
    public void SetColorHex(string? c) { ColorHex = c?.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SetIsActive(bool a) { IsActive = a; UpdatedAt = DateTime.UtcNow; }
    public void SetOffer(bool isOffer, int? discountPercent) { IsOffer = isOffer; DiscountPercent = discountPercent; UpdatedAt = DateTime.UtcNow; }
    public void SetRating(decimal? r) { Rating = r; UpdatedAt = DateTime.UtcNow; }

    public void ChangeStatus(VehicleStatus status) { Status = status; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public string FullName => $"{Brand} {Model}";
}
