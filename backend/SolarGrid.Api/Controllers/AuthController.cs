/*
 * File:        AuthController.cs
 * Author:      [Your Name] ([IT Number])
 * Description: API endpoints for authentication. The controller only receives
 *              the request and calls AuthService, which holds the logic.
 * Created:     28/09/2026
 */

using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Services;

namespace SolarGrid.Api.Controllers
{
    /// <summary>
    /// Endpoints under /api/auth.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        /// <summary>
        /// Receives the AuthService through dependency injection.
        /// </summary>
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// POST /api/auth/login - logs in with NIC and password and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
    }
}
