/*
 * File:        UsersController.cs
 * Author:      Shermon H (IT22177964)
 * Description: API endpoints for staff user management. Only Backoffice users
 *              can call these endpoints. All rules are in UserService.
 * Created:     29/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;
using SolarGrid.Api.Services;

namespace SolarGrid.Api.Controllers
{
    /// <summary>
    /// Endpoints under /api/users (Backoffice only).
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = Roles.Backoffice)]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        /// <summary>
        /// Receives the UserService through dependency injection.
        /// </summary>
        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// GET /api/users?role= - lists staff users, optionally filtered by role.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<StaffUserResponse>>> GetAll([FromQuery] string? role)
        {
            return Ok(await _userService.GetStaffAsync(role));
        }

        /// <summary>
        /// GET /api/users/{nic} - returns one staff user.
        /// </summary>
        [HttpGet("{nic}")]
        public async Task<ActionResult<StaffUserResponse>> GetByNic(string nic)
        {
            return Ok(await _userService.GetByNicAsync(nic));
        }

        /// <summary>
        /// POST /api/users - creates a Backoffice or Grid Operator user.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<StaffUserResponse>> Create(CreateStaffUserRequest request)
        {
            var user = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetByNic), new { nic = user.Nic }, user);
        }

        /// <summary>
        /// PUT /api/users/{nic} - updates a staff user.
        /// </summary>
        [HttpPut("{nic}")]
        public async Task<ActionResult<StaffUserResponse>> Update(string nic, UpdateStaffUserRequest request)
        {
            return Ok(await _userService.UpdateAsync(nic, request, User.GetNic()));
        }

        /// <summary>
        /// PATCH /api/users/{nic}/activate - activates a staff user.
        /// </summary>
        [HttpPatch("{nic}/activate")]
        public async Task<IActionResult> Activate(string nic)
        {
            await _userService.ActivateAsync(nic);
            return Ok(new { message = "User activated." });
        }

        /// <summary>
        /// PATCH /api/users/{nic}/deactivate - deactivates a staff user.
        /// </summary>
        [HttpPatch("{nic}/deactivate")]
        public async Task<IActionResult> Deactivate(string nic)
        {
            await _userService.DeactivateAsync(nic, User.GetNic());
            return Ok(new { message = "User deactivated." });
        }
    }
}
