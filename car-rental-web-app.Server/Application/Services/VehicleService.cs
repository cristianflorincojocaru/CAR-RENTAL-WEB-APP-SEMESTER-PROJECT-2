using CarRental.Application.DTOs.Vehicles;
using CarRental.Application.Interfaces;
using CarRental.Application.Mappings;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;

namespace CarRental.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public VehicleService(IUnitOfWork uow, IAuditService audit)
    {
        _uow = uow;
        _audit = audit;
    }

    public async Task<IEnumerable<VehicleDto>> GetAllAsync(VehicleFilterRequest filter, CancellationToken ct = default)
    {
        // Resolve branch name → branchId
        int? branchId = null;
        if (!string.IsNullOrWhiteSpace(filter.Branch) && filter.Branch != "All")
        {
            var branches = await _uow.Branches.GetActiveAsync(ct);
            var branch = branches.FirstOrDefault(b =>
                b.Name.Equals(filter.Branch, StringComparison.OrdinalIgnoreCase));
            branchId = branch?.Id;
        }

        VehicleCategory? category = null;
        if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "All")
            Enum.TryParse<VehicleCategory>(filter.Category, out var cat);

        if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "All"
            && Enum.TryParse<VehicleCategory>(filter.Category, ignoreCase: true, out var parsedCat))
            category = parsedCat;

        var vehicles = await _uow.Vehicles.GetByFiltersAsync(
            branchId, category, filter.PickupDate, filter.ReturnDate,
            filter.Transmission, filter.IsOffer, ct);

        return vehicles.Where(v => v.IsActive).Select(v => v.ToDto());
    }

    public async Task<VehicleDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var vehicle = await _uow.Vehicles.GetByIdWithDetailsAsync(id, ct);
        return vehicle?.ToDto();
    }

    public async Task<VehicleDto> CreateAsync(CreateVehicleRequest request, int addedByUserId, CancellationToken ct = default)
    {
        if (await _uow.Vehicles.RegistrationExistsAsync(request.RegistrationNumber, null, ct))
            throw new InvalidOperationException($"Registration number '{request.RegistrationNumber}' already exists.");

        var branch = await _uow.Branches.GetByIdAsync(request.BranchId, ct)
            ?? throw new KeyNotFoundException($"Branch {request.BranchId} not found.");

        var vehicle = Vehicle.Create(
            request.RegistrationNumber,
            request.BranchId,
            addedByUserId,
            request.Brand,
            request.Model,
            request.Year,
            request.Category,
            request.DailyRate,
            request.FuelType,
            request.Transmission,
            request.Seats,
            request.ColorHex);

        await _uow.Vehicles.AddAsync(vehicle, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(addedByUserId, "Vehicle", vehicle.Id, "VehicleAdded",
            $"Vehicle {vehicle.RegistrationNumber} ({vehicle.FullName}) added to branch {branch.Name}.", ct);

        var created = await _uow.Vehicles.GetByIdWithDetailsAsync(vehicle.Id, ct);
        return created!.ToDto();
    }

    public async Task<VehicleDto> UpdateAsync(int id, UpdateVehicleRequest request, CancellationToken ct = default)
    {
        var vehicle = await _uow.Vehicles.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found.");

        if (await _uow.Vehicles.RegistrationExistsAsync(request.RegistrationNumber, id, ct))
            throw new InvalidOperationException($"Registration number '{request.RegistrationNumber}' already in use.");

        vehicle.SetRegistrationNumber(request.RegistrationNumber);
        vehicle.SetBranchId(request.BranchId);
        vehicle.SetBrand(request.Brand);
        vehicle.SetModel(request.Model);
        vehicle.SetYear(request.Year);
        vehicle.SetCategory(request.Category);
        vehicle.SetDailyRate(request.DailyRate);
        vehicle.SetFuelType(request.FuelType);
        vehicle.SetTransmission(request.Transmission);
        vehicle.SetSeats(request.Seats);
        vehicle.SetColorHex(request.ColorHex);
        vehicle.SetIsActive(request.IsActive);

        _uow.Vehicles.Update(vehicle);
        await _uow.SaveChangesAsync(ct);

        var updated = await _uow.Vehicles.GetByIdWithDetailsAsync(id, ct);
        return updated!.ToDto();
    }

    public async Task DeactivateAsync(int id, int userId, CancellationToken ct = default)
    {
        var vehicle = await _uow.Vehicles.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found.");

        // Check no active rentals
        var hasActive = await _uow.Rentals.GetByVehicleAsync(id, ct);
        if (hasActive.Any(r => r.Status == RentalStatus.Active))
            throw new InvalidOperationException("Cannot deactivate a vehicle with active rentals.");

        vehicle.Deactivate();
        _uow.Vehicles.Update(vehicle);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "Vehicle", id, "VehicleDeactivated",
            $"Vehicle {vehicle.RegistrationNumber} deactivated.", ct);
    }

    public async Task SetOfferAsync(int id, SetOfferRequest request, int userId, CancellationToken ct = default)
    {
        var vehicle = await _uow.Vehicles.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found.");

        if (request.IsOffer && (request.DiscountPercent == null || request.DiscountPercent <= 0))
            throw new InvalidOperationException("Discount percent is required when setting an offer.");

        vehicle.SetOffer(request.IsOffer, request.IsOffer ? request.DiscountPercent : null);
        _uow.Vehicles.Update(vehicle);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "Vehicle", id, "VehicleOfferUpdated",
            $"Vehicle {vehicle.RegistrationNumber} offer set to {request.IsOffer} ({request.DiscountPercent}%).", ct);
    }

    public async Task<IEnumerable<string>> GetBranchNamesAsync(CancellationToken ct = default)
    {
        var branches = await _uow.Branches.GetActiveAsync(ct);
        return branches.Select(b => b.Name);
    }
}
