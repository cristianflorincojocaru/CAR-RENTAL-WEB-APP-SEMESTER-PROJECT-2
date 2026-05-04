namespace CarRental.Domain.Entities;

public class Rental
{
    public int Id { get; private set; }
    public int VehicleId { get; private set; }
    public int ClientId { get; private set; }
    public int BranchId { get; private set; }
    public int CreatedByUserId { get; private set; }
    public int? CompletedByUserId { get; private set; }
    public string BookingReference { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string PickupLocation { get; private set; } = string.Empty;
    public string ReturnLocation { get; private set; } = string.Empty;
    public decimal TotalCost { get; private set; }
    public RentalStatus Status { get; private set; } = RentalStatus.Active;
    public string? CancellationReason { get; private set; }
    public string? ProtectionPlan { get; private set; }
    public string? Extras { get; private set; } // JSON array
    public string? Notes { get; private set; }
    public bool PayNow { get; private set; }
    public byte[]? RowVersion { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Vehicle Vehicle { get; private set; } = null!;
    public Client Client { get; private set; } = null!;
    public Branch Branch { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;
    public User? CompletedByUser { get; private set; }

    protected Rental() { }

    public static Rental Create(int vehicleId, int clientId, int branchId, int createdByUserId,
        DateTime startDate, DateTime endDate, string pickupLocation, string returnLocation,
        decimal totalCost, string? protectionPlan = null, string? extras = null,
        string? notes = null, bool payNow = true)
    {
        return new Rental
        {
            VehicleId = vehicleId,
            ClientId = clientId,
            BranchId = branchId,
            CreatedByUserId = createdByUserId,
            BookingReference = GenerateReference(),
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            PickupLocation = pickupLocation.Trim(),
            ReturnLocation = returnLocation.Trim(),
            TotalCost = totalCost,
            Status = RentalStatus.Active,
            ProtectionPlan = protectionPlan,
            Extras = extras,
            Notes = notes?.Trim(),
            PayNow = payNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Complete(int completedByUserId)
    {
        Status = RentalStatus.Completed;
        CompletedByUserId = completedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        Status = RentalStatus.Cancelled;
        CancellationReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTotalCost(decimal cost) { TotalCost = cost; UpdatedAt = DateTime.UtcNow; }

    public int DurationDays => (EndDate - StartDate).Days;

    private static string GenerateReference()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var suffix = new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        return $"WD-{suffix}";
    }
}
