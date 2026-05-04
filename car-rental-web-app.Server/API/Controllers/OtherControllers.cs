using System.Security.Claims;
using CarRental.Application.DTOs.Auth;
using CarRental.Application.DTOs.Branches;
using CarRental.Application.DTOs.Contact;
using CarRental.Application.DTOs.Reports;
using CarRental.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

// ── Branches ───────────────────────────────────────────────────────────────

[ApiController]
[Route("api/branches")]
[Produces("application/json")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService) => _branchService = branchService;

    /// <summary>Get all active branches</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<BranchDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _branchService.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>Get branch by ID</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BranchDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _branchService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new branch (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(BranchDto), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var result = await _branchService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update branch details (Admin only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(BranchDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchRequest request, CancellationToken ct)
    {
        var result = await _branchService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Assign a manager to a branch (Admin only)</summary>
    [HttpPatch("{id:int}/assign-manager")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignManager(int id, [FromBody] AssignManagerRequest request, CancellationToken ct)
    {
        var adminId = int.Parse(User.FindFirstValue("userId")!);
        await _branchService.AssignManagerAsync(id, request.ManagerId, adminId, ct);
        return NoContent();
    }

    /// <summary>Deactivate a branch (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    [ProducesResponseType(409)]
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

    /// <summary>Get all staff users (Admin only)</summary>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _userService.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>Get user by ID</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(UserListItemDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new staff member (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserListItemDto), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var adminId = int.Parse(User.FindFirstValue("userId")!);
        var result = await _userService.CreateAsync(request, adminId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update staff member details (Admin only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserListItemDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var result = await _userService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Lock a user account (Admin only)</summary>
    [HttpPost("{id:int}/lock")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Lock(int id, CancellationToken ct)
    {
        await _userService.LockAsync(id, ct);
        return NoContent();
    }

    /// <summary>Unlock a user account (Admin only)</summary>
    [HttpPost("{id:int}/unlock")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Unlock(int id, CancellationToken ct)
    {
        await _userService.UnlockAsync(id, ct);
        return NoContent();
    }

    /// <summary>Deactivate a user (Admin only)</summary>
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

    /// <summary>Dashboard statistics (filtered by branch for managers)</summary>
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

    /// <summary>Revenue summary report</summary>
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

    /// <summary>Get unread security alerts (Admin only)</summary>
    [HttpGet("security-alerts")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> SecurityAlerts(CancellationToken ct)
    {
        var result = await _alertService.GetUnreadAlertsAsync(ct);
        return Ok(result);
    }

    /// <summary>Mark a security alert as read</summary>
    [HttpPatch("security-alerts/{id:int}/read")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAlertRead(int id, CancellationToken ct)
    {
        await _alertService.MarkAsReadAsync(id, ct);
        return NoContent();
    }

    /// <summary>Get unread security alerts count</summary>
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
    private readonly ILogger<ContactController> _logger;

    public ContactController(ILogger<ContactController> logger) => _logger = logger;

    /// <summary>Submit a contact message</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContactMessageResponse), 200)]
    public IActionResult Send([FromBody] ContactMessageRequest request)
    {
        // Log the contact request; in production send via SMTP
        _logger.LogInformation("Contact message from {Email}: {Subject}", request.Email, request.Subject);

        return Ok(new ContactMessageResponse(
            Success: true,
            Message: "Thank you! We will get back to you within 24 hours."
        ));
    }
}
