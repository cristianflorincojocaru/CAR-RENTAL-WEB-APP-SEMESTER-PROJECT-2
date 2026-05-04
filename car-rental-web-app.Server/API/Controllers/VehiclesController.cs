using System.Security.Claims;
using CarRental.Application.DTOs.Vehicles;
using CarRental.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Produces("application/json")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService) => _vehicleService = vehicleService;

    /// <summary>Get all vehicles with optional filters</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? branch,
        [FromQuery] string? category,
        [FromQuery] DateTime? pickupDate,
        [FromQuery] DateTime? returnDate,
        [FromQuery] string? transmission,
        [FromQuery] bool? isOffer,
        CancellationToken ct)
    {
        var filter = new VehicleFilterRequest(branch, category, pickupDate, returnDate, transmission, isOffer);
        var result = await _vehicleService.GetAllAsync(filter, ct);
        return Ok(result);
    }

    /// <summary>Get vehicle by ID</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VehicleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _vehicleService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Get distinct branch names</summary>
    [HttpGet("branches")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    public async Task<IActionResult> GetBranches(CancellationToken ct)
    {
        var result = await _vehicleService.GetBranchNamesAsync(ct);
        return Ok(result);
    }

    /// <summary>Add a new vehicle (Manager only)</summary>
    [HttpPost]
    [Authorize(Roles = "Manager,Administrator")]
    [ProducesResponseType(typeof(VehicleDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        var result = await _vehicleService.CreateAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a vehicle (Manager only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Manager,Administrator")]
    [ProducesResponseType(typeof(VehicleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleRequest request, CancellationToken ct)
    {
        var result = await _vehicleService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Deactivate a vehicle (Manager only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Manager,Administrator")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        await _vehicleService.DeactivateAsync(id, userId, ct);
        return NoContent();
    }

    /// <summary>Set or remove offer/discount on a vehicle (Manager only)</summary>
    [HttpPatch("{id:int}/offer")]
    [Authorize(Roles = "Manager,Administrator")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetOffer(int id, [FromBody] SetOfferRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        await _vehicleService.SetOfferAsync(id, request, userId, ct);
        return NoContent();
    }
}
