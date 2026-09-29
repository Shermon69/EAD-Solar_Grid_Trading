/*
 * File:        ReservationsController.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: API endpoints for energy reservation management.
 * Created:     29/09/2026
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Models;
using SolarGrid.Api.Services;

namespace SolarGrid.Api.Controllers
{
    /// <summary>
    /// Provides REST endpoints for creating and managing energy reservations.
    /// </summary>
    [ApiController]
    [Route("api/reservations")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService _reservationService;

        /// <summary>
        /// Receives the reservation service through dependency injection.
        /// </summary>
        public ReservationsController(ReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>
        /// Gets reservations with optional status, station and NIC filters.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = Roles.Backoffice + "," + Roles.GridOperator)]
        public async Task<ActionResult<List<ReservationResponse>>> GetReservations(
            [FromQuery] string? status = null,
            [FromQuery] string? stationId = null,
            [FromQuery] string? nic = null)
        {
            var reservations = await _reservationService.GetReservationsAsync(
                status,
                stationId,
                nic);

            return Ok(reservations);
        }

        /// <summary>
        /// Gets a single reservation by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationResponse>> GetReservation(
            string id)
        {
            var reservation = await _reservationService
                .GetReservationAsync(id);

            var currentNic = GetCurrentNic();
            var isStaff = IsStaff();

            if (!isStaff && reservation.ProsumerNic != currentNic)
            {
                return Forbid();
            }

            return Ok(reservation);
        }

        /// <summary>
        /// Creates a new energy reservation for the logged-in prosumer.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Prosumer)]
        public async Task<ActionResult<ReservationResponse>> CreateReservation(
            [FromBody] CreateReservationRequest request)
        {
            var nic = GetCurrentNic();

            var reservation = await _reservationService
                .CreateReservationAsync(request, nic);

            return CreatedAtAction(
                nameof(GetReservation),
                new { id = reservation.Id },
                reservation);
        }

        /// <summary>
        /// Updates an existing reservation.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ReservationResponse>> UpdateReservation(
            string id,
            [FromBody] UpdateReservationRequest request)
        {
            var nic = GetCurrentNic();
            var isStaff = IsStaff();

            var reservation = await _reservationService
                .UpdateReservationAsync(
                    id,
                    request,
                    nic,
                    isStaff);

            return Ok(reservation);
        }

        /// <summary>
        /// Cancels an existing reservation.
        /// </summary>
        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<ReservationResponse>> CancelReservation(
            string id)
        {
            var nic = GetCurrentNic();
            var isStaff = IsStaff();

            var reservation = await _reservationService
                .CancelReservationAsync(
                    id,
                    nic,
                    isStaff);

            return Ok(reservation);
        }

        /// <summary>
        /// Approves a pending reservation and generates its QR token.
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = Roles.Backoffice + "," + Roles.GridOperator)]
        public async Task<ActionResult<ReservationResponse>> ApproveReservation(
            string id)
        {
            var reservation = await _reservationService
                .ApproveReservationAsync(id);

            return Ok(reservation);
        }

        /// <summary>
        /// Gets the NIC of the currently authenticated user from the JWT.
        /// </summary>
        private string GetCurrentNic()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "User identity was not found.");
        }

        /// <summary>
        /// Checks whether the current user is a Backoffice user
        /// or Grid Operator.
        /// </summary>
        private bool IsStaff()
        {
            return User.IsInRole(Roles.Backoffice) ||
                   User.IsInRole(Roles.GridOperator);
        }
    }
}