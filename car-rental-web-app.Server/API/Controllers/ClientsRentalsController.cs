using System.Security.Claims;
using CarRental.Application.DTOs.Clients;
using CarRental.Application.DTOs.Rentals;
using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
[Produces("application/json")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService) => _clientService = clientService;

    /// <summary>Search and list all clients</summary>
    [HttpGet]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct)
    {
        var result = await _clientService.GetAllAsync(search, ct);
        return Ok(result);
    }

    /// <summary>Get client by ID</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    [ProducesResponseType(typeof(ClientDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _clientService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new client</summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    [ProducesResponseType(typeof(ClientDto), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken ct)
    {
        var result = await _clientService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update client details</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(ClientDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request, CancellationToken ct)
    {
        var result = await _clientService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Flag a client as problematic</summary>
    [HttpPost("{id:int}/flag")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Flag(int id, [FromBody] FlagClientRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        await _clientService.FlagAsync(id, request, userId, ct);
        return NoContent();
    }

    /// <summary>Remove flag from a client</summary>
    [HttpDelete("{id:int}/flag")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Unflag(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        await _clientService.UnflagAsync(id, userId, ct);
        return NoContent();
    }

    /// <summary>Get rental history for a client</summary>
    [HttpGet("{id:int}/rentals")]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    [ProducesResponseType(typeof(IEnumerable<RentalListItemDto>), 200)]
    public async Task<IActionResult> GetRentalHistory(int id, CancellationToken ct)
    {
        var result = await _clientService.GetRentalHistoryAsync(id, ct);
        return Ok(result);
    }
}

// ── Rentals Controller ─────────────────────────────────────────────────────

[ApiController]
[Route("api/rentals")]
[Authorize]
[Produces("application/json")]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService) => _rentalService = rentalService;

    /// <summary>Get all rentals with optional filters</summary>
    [HttpGet]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    [ProducesResponseType(typeof(IEnumerable<RentalListItemDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        RentalStatus? rentalStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RentalStatus>(status, true, out var parsed))
            rentalStatus = parsed;

        var result = await _rentalService.GetAllAsync(branchId, rentalStatus, from, to, ct);
        return Ok(result);
    }

    /// <summary>Get rentals for current logged-in client</summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<RentalListItemDto>), 200)]
    public async Task<IActionResult> GetMyRentals(CancellationToken ct)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        var result = await _rentalService.GetByClientEmailAsync(email, ct);
        return Ok(result);
    }

    /// <summary>Get rental by ID</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    [ProducesResponseType(typeof(RentalDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _rentalService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new rental contract</summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    [ProducesResponseType(typeof(RentalDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateRentalRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        var result = await _rentalService.CreateAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Mark a rental as completed</summary>
    [HttpPatch("{id:int}/complete")]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    [ProducesResponseType(typeof(RentalDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        var result = await _rentalService.CompleteAsync(id, userId, ct);
        return Ok(result);
    }

    /// <summary>Cancel a rental</summary>
    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(RentalDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelRentalRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        var result = await _rentalService.CancelAsync(id, request.Reason, userId, ct);
        return Ok(result);
    }
}