/*
 * File: ProsumersController.cs
 * Author: Premaratne R.A.N.C (IT22050908)
 * Description: REST endpoints for prosumer registration, management, profile, bookings and dashboards.
 *              Contains no business rules; it only calls ProsumerService.
 * Created: 29/09/2026
 */
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.Dtos;
using SolarGrid.Api.Services;

namespace SolarGrid.Api.Controllers;

[ApiController]
[Route("api/prosumers")]
public class ProsumersController : ControllerBase
{
    private readonly ProsumerService _service;

    // Receives the service through dependency injection
    public ProsumersController(ProsumerService service) => _service = service;

    // NIC of the logged-in user, read from the JWT (Member 1 puts the NIC in the NameIdentifier claim)
    private string CurrentNic => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // POST api/prosumers/register - public registration, account starts as Pending
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var created = await _service.RegisterAsync(request);
        if (!created) return Conflict(new { message = "This NIC is already registered." });
        return Ok(new { message = "Registered. Your account is pending activation by Backoffice." });
    }

    // GET api/prosumers?search=&status= - Backoffice: list/search prosumers
    [HttpGet]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status)
        => Ok(await _service.GetProsumersAsync(search, status));

    // GET api/prosumers/pending - Backoffice: prosumers waiting for activation
    [HttpGet("pending")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> GetPending()
        => Ok(await _service.GetProsumersAsync(null, "Pending"));

    // GET api/prosumers/dashboard - Backoffice: pending count + approved future reservations count
    [HttpGet("dashboard")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> BackofficeDashboard()
        => Ok(await _service.GetBackofficeDashboardAsync());

    // GET api/prosumers/{nic} - Backoffice: one prosumer
    [HttpGet("{nic}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> GetOne(string nic)
    {
        var p = await _service.GetByNicAsync(nic);
        return p == null ? NotFound(new { message = "Prosumer not found." }) : Ok(p);
    }

    // PUT api/prosumers/{nic} - Backoffice: edit a prosumer's details
    [HttpPut("{nic}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Update(string nic, [FromBody] UpdateProfileRequest request)
    {
        var ok = await _service.UpdateProfileAsync(nic, request);
        return ok ? Ok(new { message = "Prosumer updated." }) : NotFound(new { message = "Prosumer not found." });
    }

    // PUT api/prosumers/{nic}/activate - Backoffice: approve pending or reactivate deactivated
    [HttpPut("{nic}/activate")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Activate(string nic)
    {
        var ok = await _service.ActivateAsync(nic);
        return ok ? Ok(new { message = "Prosumer activated." }) : NotFound(new { message = "Prosumer not found or already active." });
    }

    // PUT api/prosumers/{nic}/deactivate - Backoffice: deactivate an account
    [HttpPut("{nic}/deactivate")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Deactivate(string nic)
    {
        var ok = await _service.DeactivateAsync(nic);
        return ok ? Ok(new { message = "Prosumer deactivated." }) : NotFound(new { message = "Prosumer not found or already deactivated." });
    }

    // GET api/prosumers/me/profile - Prosumer: own profile
    [HttpGet("me/profile")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> MyProfile()
    {
        var p = await _service.GetByNicAsync(CurrentNic);
        return p == null ? NotFound(new { message = "Profile not found." }) : Ok(p);
    }

    // PUT api/prosumers/me/profile - Prosumer: edit own profile
    [HttpPut("me/profile")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var ok = await _service.UpdateProfileAsync(CurrentNic, request);
        return ok ? Ok(new { message = "Profile updated." }) : NotFound(new { message = "Profile not found." });
    }

    // POST api/prosumers/me/request-deactivation - Prosumer: ask Backoffice to deactivate the account
    [HttpPost("me/request-deactivation")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> RequestDeactivation()
    {
        var ok = await _service.RequestDeactivationAsync(CurrentNic);
        return ok ? Ok(new { message = "Deactivation requested. Backoffice will review it." })
                  : BadRequest(new { message = "Only active accounts can request deactivation." });
    }

    // GET api/prosumers/me/bookings?type=current|history&search= - Prosumer: own bookings
    [HttpGet("me/bookings")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> MyBookings([FromQuery] string type = "current", [FromQuery] string? search = null)
        => Ok(await _service.GetMyBookingsAsync(CurrentNic, type, search));

    // GET api/prosumers/me/dashboard - Prosumer: own pending count + approved future count
    [HttpGet("me/dashboard")]
    [Authorize(Roles = "Prosumer")]
    public async Task<IActionResult> MyDashboard()
        => Ok(await _service.GetMyDashboardAsync(CurrentNic));
}
