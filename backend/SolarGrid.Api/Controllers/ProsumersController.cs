/*
 * File:        ProsumersController.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: REST endpoints for prosumer registration, management, profile,
 *              bookings and dashboards. Contains no business rules; it only
 *              calls ProsumerService.
 * Created:     29/09/2026
 */
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.Dtos;
using SolarGrid.Api.Services;

namespace SolarGrid.Api.Controllers;

/// <summary>
/// REST controller for all prosumer operations. Acts as a thin UI layer —
/// validates HTTP input and delegates all business logic to ProsumerService.
/// </summary>
[ApiController]
[Route("api")] 
public class ProsumersController : ControllerBase
{
    private readonly ProsumerService _service;
    public ProsumersController(ProsumerService service) => _service = service;
    private string CurrentNic => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    /// <summary>Registers a new prosumer (public). Account starts as Pending.</summary>
    [HttpPost("auth/register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var created = await _service.RegisterAsync(request);
        if (!created) return Conflict(new { message = "This NIC is already registered." });
        return Ok(new { message = "Registered. Your account is pending activation by Backoffice." });
    }

    /// <summary>Backoffice: list/search prosumers by search text and status.</summary>
    [HttpGet("prosumers")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status)
        => Ok(await _service.GetProsumersAsync(search, status));

    /// <summary>Backoffice: prosumers waiting for activation.</summary>
    [HttpGet("prosumers/pending")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> GetPending()
        => Ok(await _service.GetProsumersAsync(null, "Pending"));

    /// <summary>Backoffice: pending count + approved future reservations count.</summary>
    [HttpGet("dashboard/operator")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> BackofficeDashboard()
        => Ok(await _service.GetBackofficeDashboardAsync());

    /// <summary>Backoffice: get one prosumer by NIC.</summary>
    [HttpGet("prosumers/{nic}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> GetOne(string nic)
    {
        var p = await _service.GetByNicAsync(nic);
        return p == null ? NotFound(new { message = "Prosumer not found." }) : Ok(p);
    }

    /// <summary>Backoffice: update a prosumer's profile details.</summary>
    [HttpPut("prosumers/{nic}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Update(string nic, [FromBody] UpdateProfileRequest request)
    {
        var ok = await _service.UpdateProfileAsync(nic, request);
        return ok ? Ok(new { message = "Prosumer updated." }) : NotFound(new { message = "Prosumer not found." });
    }

    /// <summary>Backoffice: activate a pending or reactivate a deactivated account.</summary>
    [HttpPatch("prosumers/{nic}/activate")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Activate(string nic)
    {
        var ok = await _service.ActivateAsync(nic);
        return ok ? Ok(new { message = "Prosumer activated." }) : NotFound(new { message = "Prosumer not found or already active." });
    }

    /// <summary>Backoffice: deactivate an active prosumer account.</summary>
    [HttpPatch("prosumers/{nic}/deactivate")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Deactivate(string nic)
    {
        var ok = await _service.DeactivateAsync(nic);
        return ok ? Ok(new { message = "Prosumer deactivated." }) : NotFound(new { message = "Prosumer not found or already deactivated." });
    }

    /// <summary>Prosumer: get own profile.</summary>
    [HttpGet("profile")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> MyProfile()
    {
        var p = await _service.GetByNicAsync(CurrentNic);
        return p == null ? NotFound(new { message = "Profile not found." }) : Ok(p);
    }

    /// <summary>Prosumer: update own profile.</summary>
    [HttpPut("profile")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var ok = await _service.UpdateProfileAsync(CurrentNic, request);
        return ok ? Ok(new { message = "Profile updated." }) : NotFound(new { message = "Profile not found." });
    }

    /// <summary>Prosumer: request account deactivation (Backoffice approval required).</summary>
    [HttpPost("profile/deactivate-request")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> RequestDeactivation()
    {
        var ok = await _service.RequestDeactivationAsync(CurrentNic);
        return ok ? Ok(new { message = "Deactivation requested. Backoffice will review it." })
                  : BadRequest(new { message = "Only active accounts can request deactivation." });
    }

    /// <summary>Prosumer: own bookings (type = current | history, with optional search).</summary>
    [HttpGet("my/reservations")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> MyBookings([FromQuery] string type = "current", [FromQuery] string? search = null)
        => Ok(await _service.GetMyBookingsAsync(CurrentNic, type, search));

    /// <summary>Prosumer: own dashboard — pending + approved future counts.</summary>
    [HttpGet("dashboard/prosumer")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> MyDashboard()
        => Ok(await _service.GetMyDashboardAsync(CurrentNic));
}