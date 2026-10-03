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
            // Retrieves reservations based on the provided status, station and NIC filters.
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
            // Retrieves the reservation details using the reservation ID.
            var reservation = await _reservationService
                .GetReservationAsync(id);

            // Gets the authenticated user's NIC and checks whether the user is staff.
            var currentNic = GetCurrentNic();
            var isStaff = IsStaff();

            // Prevents a prosumer from accessing another prosumer's reservation.
            if (!isStaff && reservation.ProsumerNic != currentNic)
            {
                return Forbid();
            }

            return Ok(reservation);
        }

        /// <summary>
        /// Creates a new energy reservation.
        /// Prosumer users create reservations for themselves,
        /// while staff users can create reservations for a selected prosumer.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReservationResponse>> CreateReservation(
            [FromBody] CreateReservationRequest request)
        {
            // Determines the prosumer NIC based on whether the authenticated user is staff or a prosumer.
            var isStaff = IsStaff();

            var nic = isStaff
                ? request.ProsumerNic
                : GetCurrentNic();

            // Validates that a prosumer NIC is available before creating the reservation.
            if (string.IsNullOrWhiteSpace(nic))
            {
                return BadRequest(new
                {
                    message = "Prosumer NIC is required."
                });
            }

            // Creates the reservation using the reservation service.
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
            // Gets the authenticated user's NIC and determines whether the user is staff.
            var nic = GetCurrentNic();
            var isStaff = IsStaff();

            // Updates the reservation using the reservation ID and authenticated user details.
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
            // Gets the authenticated user's NIC and determines whether the user is staff.
            var nic = GetCurrentNic();
            var isStaff = IsStaff();

            // Cancels the reservation after validating the user's permissions and business rules.
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
            // Approves the pending reservation and generates a QR token through the reservation service.
            var reservation = await _reservationService
                .ApproveReservationAsync(id);

            return Ok(reservation);
        }

        /// <summary>
        /// Gets the NIC of the currently authenticated user from the JWT.
        /// </summary>
        private string GetCurrentNic()
        {
            // Retrieves the authenticated user's NIC from the NameIdentifier claim in the JWT.
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
            // Checks whether the authenticated user has either the Backoffice or Grid Operator role.
            return User.IsInRole(Roles.Backoffice) ||
                   User.IsInRole(Roles.GridOperator);
        }
    }
}