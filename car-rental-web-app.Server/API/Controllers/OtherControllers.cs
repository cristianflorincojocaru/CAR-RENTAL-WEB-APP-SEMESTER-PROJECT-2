using System.Security.Claims;
using CarRental.Application.DTOs.Auth;
using CarRental.Application.DTOs.Branches;
using CarRental.Application.DTOs.Contact;
using CarRental.Application.DTOs.Promo;
using CarRental.Application.DTOs.Reports;
using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.API.Controllers;

// ── Branches ───────────────────────────────────────────────────────────────

[ApiController]
[Route("api/branches")]
[Produces("application/json")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService) => _branchService = branchService;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<BranchDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _branchService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BranchDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _branchService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(BranchDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var result = await _branchService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(BranchDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchRequest request, CancellationToken ct)
    {
        var result = await _branchService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPatch("{id:int}/assign-manager")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> AssignManager(int id, [FromBody] AssignManagerRequest request, CancellationToken ct)
    {
        var adminId = int.Parse(User.FindFirstValue("userId")!);
        await _branchService.AssignManagerAsync(id, request.ManagerId, adminId, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var adminId = int.Parse(User.FindFirstValue("userId")!);
        await _branchService.DeactivateAsync(id, adminId, ct);
        return NoContent();
    }
}

// ── Users / Staff ──────────────────────────────────────────────────────────

[ApiController]
[Route("api/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _userService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(UserListItemDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserListItemDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var adminId = int.Parse(User.FindFirstValue("userId")!);
        var result = await _userService.CreateAsync(request, adminId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserListItemDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var result = await _userService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/lock")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Lock(int id, CancellationToken ct)
    {
        await _userService.LockAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/unlock")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Unlock(int id, CancellationToken ct)
    {
        await _userService.UnlockAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await _userService.DeactivateAsync(id, ct);
        return NoContent();
    }
}

// ── Reports ────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Administrator,Manager")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ISecurityAlertService _alertService;

    public ReportsController(IReportService reportService, ISecurityAlertService alertService)
    {
        _reportService = reportService;
        _alertService = alertService;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardStatsDto), 200)]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        int? branchId = null;
        if (role == "Manager")
        {
            var branchClaim = User.FindFirstValue("branchId");
            if (branchClaim != null) branchId = int.Parse(branchClaim);
        }
        var result = await _reportService.GetDashboardStatsAsync(branchId, ct);
        return Ok(result);
    }

    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueSummaryDto), 200)]
    public async Task<IActionResult> Revenue(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? branchId,
        CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Manager")
        {
            var branchClaim = User.FindFirstValue("branchId");
            branchId = branchClaim != null ? int.Parse(branchClaim) : branchId;
        }
        var result = await _reportService.GetRevenueSummaryAsync(from, to, branchId, ct);
        return Ok(result);
    }

    [HttpGet("security-alerts")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> SecurityAlerts(CancellationToken ct)
    {
        var result = await _alertService.GetUnreadAlertsAsync(ct);
        return Ok(result);
    }

    [HttpPatch("security-alerts/{id:int}/read")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAlertRead(int id, CancellationToken ct)
    {
        await _alertService.MarkAsReadAsync(id, ct);
        return NoContent();
    }

    [HttpGet("security-alerts/count")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> AlertCount(CancellationToken ct)
    {
        var count = await _alertService.GetUnreadCountAsync(ct);
        return Ok(new { count });
    }
}

// ── Contact ────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/contact")]
[AllowAnonymous]
[Produces("application/json")]
public class ContactController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContactController> _logger;

    public ContactController(AppDbContext db, ILogger<ContactController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactMessageResponse), 200)]
    public async Task<IActionResult> Send([FromBody] ContactMessageRequest request, CancellationToken ct)
    {
        var message = ContactMessage.Create(
            request.FirstName, request.LastName, request.Email,
            request.Phone, request.Subject, request.Message);

        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Contact message saved from {Email}: {Subject}", request.Email, request.Subject);

        return Ok(new ContactMessageResponse(
            Success: true,
            Message: "Thank you! We will get back to you within 24 hours."
        ));
    }
}

// ── Promo ──────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/promo")]
[AllowAnonymous]
[Produces("application/json")]
public class PromoController : ControllerBase
{
    private readonly AppDbContext _db;

    public PromoController(AppDbContext db) => _db = db;

    /// <summary>Validate a promo code for a specific vehicle and dates</summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(PromoValidationResult), 200)]
    public async Task<IActionResult> Validate([FromBody] ValidatePromoRequest request, CancellationToken ct)
    {
        var code = await _db.PromoCodes
            .FirstOrDefaultAsync(p => p.Code == request.Code.ToUpper().Trim(), ct);

        if (code == null || !code.IsValid())
            return Ok(new PromoValidationResult(false, "Invalid or expired promo code.", null, null, null));

        // Verifică condiția de weekend
        if (code.WeekendOnly)
        {
            if (!DateTime.TryParse(request.PickupDate, out var pickup))
                return Ok(new PromoValidationResult(false, "Invalid pickup date.", null, null, null));

            var isWeekend = pickup.DayOfWeek == DayOfWeek.Saturday || pickup.DayOfWeek == DayOfWeek.Sunday;
            if (!isWeekend)
                return Ok(new PromoValidationResult(false, "This code is valid for weekend rentals only.", null, null, null));
        }

        // Verifică condiția de categorie
        if (!string.IsNullOrEmpty(code.ApplicableCategory))
        {
            var vehicle = await _db.Vehicles.FindAsync(new object[] { request.VehicleId }, ct);
            if (vehicle == null || vehicle.Category.ToString() != code.ApplicableCategory)
                return Ok(new PromoValidationResult(false, $"This code is only valid for {code.ApplicableCategory} vehicles.", null, null, null));
        }

        // Verifică condiția de vehicul specific
        if (code.ApplicableVehicleId.HasValue && code.ApplicableVehicleId != request.VehicleId)
            return Ok(new PromoValidationResult(false, "This code is not valid for this vehicle.", null, null, null));

        return Ok(new PromoValidationResult(true, null, code.Code, code.DiscountPercent, code.Description));
    }
}