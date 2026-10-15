/*
 * File:        QrVerificationService.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Business logic for Grid Operator QR verification and completion
 *              of energy reservations (R9). The QR code on the prosumer's phone
 *              holds "reservationId|qrToken"; the token is compared with the one
 *              saved when the booking was approved.
 */

using MongoDB.Driver;
using SolarGrid.Api.Data;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services
{
    /// <summary>
    /// Verifies scanned reservation QR codes and completes energy transfers.
    /// </summary>
    public class QrVerificationService
    {
        private readonly IMongoCollection<EnergyReservation> _reservations;
        private readonly IMongoCollection<SolarStation> _stations;

        /// <summary>
        /// Gets the reservation and station collections from the database context.
        /// </summary>
        public QrVerificationService(MongoDbContext context)
        {
            _reservations = context.Reservations;
            _stations = context.Stations;
        }

        /// <summary>
        /// Rule R9: checks a scanned QR code. The reservation must exist, be Approved
        /// and have the same QR token. Returns the reservation details for the operator.
        /// </summary>
        public async Task<ReservationResponse> VerifyQrAsync(QrVerifyRequest dto)
        {
            var reservation = await FindReservationAsync(dto.ReservationId);

            // Compare the token first, so a guessed reservation ID reveals nothing about its status
            if (string.IsNullOrEmpty(reservation.QrToken) || reservation.QrToken != dto.QrToken)
            {
                throw new BusinessRuleException("Invalid QR code. It does not match this reservation.");
            }

            EnsureApproved(reservation);

            return await MapToResponseAsync(reservation);
        }

        /// <summary>
        /// Rule R9: completes the energy transfer after a successful scan. Sets the
        /// status to Completed and records which grid operator finished it and when.
        /// </summary>
        public async Task<ReservationResponse> CompleteAsync(string reservationId, string operatorNic)
        {
            var reservation = await FindReservationAsync(reservationId);
            EnsureApproved(reservation);

            reservation.Status = ReservationStatus.Completed;
            reservation.CompletedBy = operatorNic;
            reservation.CompletedAt = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            // Only update if it is still Approved, so two operators cannot complete it twice
            var result = await _reservations.ReplaceOneAsync(
                r => r.Id == reservationId && r.Status == ReservationStatus.Approved,
                reservation);

            if (result.ModifiedCount == 0)
            {
                throw BusinessRuleException.Conflict("This reservation was already completed or changed. Please scan again.");
            }

            return await MapToResponseAsync(reservation);
        }

        /// <summary>
        /// Loads a reservation by ID, or throws 404 if the ID is unknown or not a valid ID.
        /// </summary>
        private async Task<EnergyReservation> FindReservationAsync(string reservationId)
        {
            // MongoDB IDs are 24 hex characters; anything else cannot be a reservation
            if (!MongoDB.Bson.ObjectId.TryParse(reservationId, out _))
            {
                throw BusinessRuleException.NotFound("Reservation not found.");
            }

            var reservation = await _reservations.Find(r => r.Id == reservationId).FirstOrDefaultAsync();
            if (reservation == null)
            {
                throw BusinessRuleException.NotFound("Reservation not found.");
            }
            return reservation;
        }

        /// <summary>
        /// Throws 409 with a clear message when the reservation is not in the Approved state.
        /// </summary>
        private static void EnsureApproved(EnergyReservation reservation)
        {
            if (reservation.Status == ReservationStatus.Approved)
            {
                return;
            }

            var message = reservation.Status switch
            {
                ReservationStatus.Completed => "This energy transfer has already been completed.",
                ReservationStatus.Cancelled => "This reservation was cancelled.",
                ReservationStatus.Pending => "This reservation has not been approved yet.",
                _ => $"Reservation is not Approved. Current status: {reservation.Status}"
            };
            throw BusinessRuleException.Conflict(message);
        }

        /// <summary>
        /// Builds the response sent to the mobile app, adding the station name.
        /// The QR token is left out because the operator does not need it.
        /// </summary>
        private async Task<ReservationResponse> MapToResponseAsync(EnergyReservation reservation)
        {
            var station = await _stations.Find(s => s.Id == reservation.StationId).FirstOrDefaultAsync();

            return new ReservationResponse
            {
                Id = reservation.Id ?? string.Empty,
                ProsumerNic = reservation.ProsumerNic,
                StationId = reservation.StationId,
                StationName = station?.Name ?? "Unknown Station",
                SlotId = reservation.SlotId,
                ReservationTime = reservation.ReservationTime,
                Type = reservation.Type,
                EnergyKwh = reservation.EnergyKwh,
                Status = reservation.Status,
                QrToken = null,
                CompletedBy = reservation.CompletedBy,
                CreatedAt = reservation.CreatedAt,
                UpdatedAt = reservation.UpdatedAt,
                CompletedAt = reservation.CompletedAt
            };
        }
    }
}
