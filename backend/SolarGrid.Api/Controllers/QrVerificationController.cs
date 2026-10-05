/*
 * File:        QrVerificationController.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: API endpoints for QR code verification and completing reservations.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Models;
using SolarGrid.Api.Services;
using System.Security.Claims;

namespace SolarGrid.Api.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class QrVerificationController : ControllerBase
    {
        private readonly QrVerificationService _qrService;

        /// <summary>
        /// Initializes a new instance of QrVerificationController.
        /// </summary>
        public QrVerificationController(QrVerificationService qrService)
        {
            _qrService = qrService;
        }

        [HttpPost("verify-qr")]
        [Authorize(Roles = Roles.GridOperator)]
        /// <summary>
        /// Executes the VerifyQr operation.
        /// </summary>
        public async Task<IActionResult> VerifyQr([FromBody] QrVerifyRequest dto)
        {
            var reservation = await _qrService.VerifyQrAsync(dto);
            return Ok(reservation);
        }

        [HttpPatch("{id}/complete")]
        [Authorize(Roles = Roles.GridOperator)]
        /// <summary>
        /// Executes the Complete operation.
        /// </summary>
        public async Task<IActionResult> Complete(string id)
        {
            // Extract the NIC of the logged-in Grid Operator from the JWT token
            var operatorNic = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            
            var reservation = await _qrService.CompleteAsync(id, operatorNic);
            return Ok(reservation);
        }
    }
}
