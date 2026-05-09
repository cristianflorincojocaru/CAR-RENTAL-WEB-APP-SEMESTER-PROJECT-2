namespace CarRental.Domain.Entities;

public enum UserRole
{
    Administrator = 0,
    Manager = 1,
    Operator = 2
}

public enum VehicleCategory
{
    Economy = 0,
    Compact = 1,
    SUV = 2,
    Premium = 3,
    Van = 4
}

public enum VehicleStatus
{
    Available = 0,
    Rented = 1,
    Maintenance = 2
}

public enum RentalStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

public enum AlertType
{
    FailedLoginThreshold = 0,
    AccountLocked = 1,
    UnauthorizedAccess = 2
}

public enum PromoType { Percentage }