using System.Text.Json;
using CarRental.Application.DTOs.Rentals;
using CarRental.Application.Interfaces;
using CarRental.Application.Mappings;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;

namespace CarRental.Application.Services;

public class RentalService : IRentalService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public RentalService(IUnitOfWork uow, IAuditService audit)
    {
        _uow = uow;
        _audit = audit;
    }

    public async Task<IEnumerable<RentalListItemDto>> GetAllAsync(int? branchId, RentalStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var rentals = await _uow.Rentals.GetByFiltersAsync(branchId, status, from, to, ct);
        return rentals.OrderByDescending(r => r.CreatedAt).Select(r => r.ToListDto());
    }

    public async Task<RentalDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var rental = await _uow.Rentals.GetByIdWithDetailsAsync(id, ct);
        return rental?.ToDto();
    }

    public async Task<RentalDto> CreateAsync(CreateRentalRequest request, int createdByUserId, CancellationToken ct = default)
    {
        // Validate dates
        if (request.PickupDate.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Pick-up date cannot be in the past.");

        if (request.ReturnDate.Date <= request.PickupDate.Date)
            throw new InvalidOperationException("Return date must be after pick-up date.");

        // Validate vehicle
        var vehicle = await _uow.Vehicles.GetByIdWithDetailsAsync(request.VehicleId, ct)
            ?? throw new KeyNotFoundException($"Vehicle {request.VehicleId} not found.");

        if (!vehicle.IsActive)
            throw new InvalidOperationException("Vehicle is not active.");

        if (vehicle.Status == VehicleStatus.Maintenance)
            throw new InvalidOperationException("Vehicle is currently under maintenance.");

        // Check overlap
        var hasOverlap = await _uow.Vehicles.HasOverlapAsync(
            request.VehicleId, request.PickupDate, request.ReturnDate, null, ct);

        if (hasOverlap)
            throw new InvalidOperationException("Vehicle is not available for the selected period.");

        // Find or create client
        var client = await _uow.Clients.GetByEmailAsync(request.Email, ct);
        if (client == null)
        {
            client = Client.Create($"{request.FirstName} {request.LastName}", request.Email, request.Phone);
            await _uow.Clients.AddAsync(client, ct);
            await _uow.SaveChangesAsync(ct);
        }

        if (client.IsFlagged)
            throw new InvalidOperationException("Client is flagged and cannot make new rentals.");

        if (!client.IsActive)
            throw new InvalidOperationException("Client account is inactive.");

        // Get branch from user
        var createdByUser = await _uow.Users.GetByIdAsync(createdByUserId, ct)
            ?? throw new KeyNotFoundException("Creating user not found.");

        var branchId = createdByUser.BranchId ?? vehicle.BranchId;

        // Calculate cost
        var days = (request.ReturnDate.Date - request.PickupDate.Date).Days;
        var baseRate = vehicle.IsOffer && vehicle.DiscountPercent.HasValue
            ? vehicle.DailyRate * (1 - vehicle.DiscountPercent.Value / 100m)
            : vehicle.DailyRate;

        var protectionCost = request.ProtectionPlan?.ToLower() switch
        {
            "standard" => 8m,
            "premium" => 16m,
            _ => 0m
        };

        var extrasCost = CalculateExtrasCost(request.Extras, days);
        var totalCost = Math.Round((baseRate + protectionCost + (extrasCost / days)) * days, 2);

        var extrasJson = request.Extras?.Count > 0
            ? JsonSerializer.Serialize(request.Extras)
            : null;

        var rental = Rental.Create(
            vehicle.Id,
            client.Id,
            branchId,
            createdByUserId,
            request.PickupDate,
            request.ReturnDate,
            request.PickupLocation,
            request.ReturnLocation,
            totalCost,
            request.ProtectionPlan,
            extrasJson,
            request.Notes,
            request.PayNow);

        // Update vehicle status
        vehicle.ChangeStatus(VehicleStatus.Rented);
        _uow.Vehicles.Update(vehicle);

        await _uow.Rentals.AddAsync(rental, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(createdByUserId, "Rental", rental.Id, "RentalCreated",
            $"Rental {rental.BookingReference} created for client {client.FullName}, vehicle {vehicle.RegistrationNumber}.", ct);

        var created = await _uow.Rentals.GetByIdWithDetailsAsync(rental.Id, ct);
        return created!.ToDto();
    }

    public async Task<RentalDto> CompleteAsync(int id, int completedByUserId, CancellationToken ct = default)
    {
        var rental = await _uow.Rentals.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Rental {id} not found.");

        if (rental.Status != RentalStatus.Active)
            throw new InvalidOperationException($"Rental is not active (current status: {rental.Status}).");

        rental.Complete(completedByUserId);
        _uow.Rentals.Update(rental);

        // Set vehicle back to available
        var vehicle = await _uow.Vehicles.GetByIdAsync(rental.VehicleId, ct);
        if (vehicle != null)
        {
            vehicle.ChangeStatus(VehicleStatus.Available);
            _uow.Vehicles.Update(vehicle);
        }

        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(completedByUserId, "Rental", id, "RentalCompleted",
            $"Rental {rental.BookingReference} completed.", ct);

        var updated = await _uow.Rentals.GetByIdWithDetailsAsync(id, ct);
        return updated!.ToDto();
    }

    public async Task<RentalDto> CancelAsync(int id, string reason, int userId, CancellationToken ct = default)
    {
        var rental = await _uow.Rentals.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Rental {id} not found.");

        if (rental.Status != RentalStatus.Active)
            throw new InvalidOperationException($"Rental cannot be cancelled (current status: {rental.Status}).");

        rental.Cancel(reason);
        _uow.Rentals.Update(rental);

        // Set vehicle back to available
        var vehicle = await _uow.Vehicles.GetByIdAsync(rental.VehicleId, ct);
        if (vehicle != null)
        {
            vehicle.ChangeStatus(VehicleStatus.Available);
            _uow.Vehicles.Update(vehicle);
        }

        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "Rental", id, "RentalCancelled",
            $"Rental {rental.BookingReference} cancelled. Reason: {reason}", ct);

        var updated = await _uow.Rentals.GetByIdWithDetailsAsync(id, ct);
        return updated!.ToDto();
    }

    public async Task<IEnumerable<RentalListItemDto>> GetByClientAsync(int clientId, CancellationToken ct = default)
    {
        var rentals = await _uow.Rentals.GetByClientAsync(clientId, ct);
        return rentals.OrderByDescending(r => r.StartDate).Select(r => r.ToListDto());
    }

    private static decimal CalculateExtrasCost(List<string>? extras, int days)
    {
        if (extras == null || extras.Count == 0) return 0;

        var ratesPerDay = new Dictionary<string, decimal>
        {
            ["gps"] = 4m,
            ["child_seat"] = 5m,
            ["additional_driver"] = 7m,
            ["roadside"] = 3m,
            ["full_insurance"] = 0m
        };

        return extras
            .Where(e => ratesPerDay.ContainsKey(e))
            .Sum(e => ratesPerDay[e] * days);
    }
}
