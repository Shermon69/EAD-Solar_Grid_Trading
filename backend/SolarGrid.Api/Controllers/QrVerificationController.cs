/*
 * File:        QrVerificationController.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: API endpoints used by the Grid Operator mobile app to verify a
 *              prosumer's reservation QR code and complete the energy transfer.
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
    /// QR verification endpoints (rule R9). Only grid operators can use them.
    /// </summary>
    [ApiController]
    [Route("api/reservations")]
    public class QrVerificationController : ControllerBase
    {
        private readonly QrVerificationService _qrService;

        /// <summary>
        /// Receives the QR verification service through dependency injection.
        /// </summary>
        public QrVerificationController(QrVerificationService qrService)
        {
            _qrService = qrService;
        }

        /// <summary>
        /// POST /api/reservations/verify-qr - checks the reservation ID and QR token
        /// read from the prosumer's QR code and returns the reservation details.
        /// </summary>
        [HttpPost("verify-qr")]
        [Authorize(Roles = Roles.GridOperator)]
        public async Task<IActionResult> VerifyQr([FromBody] QrVerifyRequest dto)
        {
            var reservation = await _qrService.VerifyQrAsync(dto);
            return Ok(reservation);
        }

        /// <summary>
        /// PATCH /api/reservations/{id}/complete - marks a verified reservation as
        /// Completed and records the logged-in operator's NIC.
        /// </summary>
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = Roles.GridOperator)]
        public async Task<IActionResult> Complete(string id)
        {
            var reservation = await _qrService.CompleteAsync(id, User.GetNic());
            return Ok(reservation);
        }
    }
}
