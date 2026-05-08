namespace CarRental.Application.DTOs.Reports;

public record RevenueSummaryDto(
    decimal TotalRevenue,
    int TotalRentals,
    int ActiveRentals,
    int CompletedRentals,
    int CancelledRentals,
    decimal AverageDailyRate,
    Dictionary<string, decimal> RevenueByBranch,
    Dictionary<string, int> RentalsByCategory
);

public record DashboardStatsDto(
    int TotalVehicles,
    int AvailableVehicles,
    int RentedVehicles,
    int TotalClients,
    int ActiveRentals,
    int CompletedToday,
    decimal TodayRevenue,
    int UnreadAlerts
);
