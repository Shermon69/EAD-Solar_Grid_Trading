/*
 * File:        QrVerificationService.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Business logic for Grid Operator QR verification and completion
 *              of energy reservations (R9).
 */

using MongoDB.Driver;
using SolarGrid.Api.Data;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services
{
    public class QrVerificationService
    {
        private readonly IMongoCollection<EnergyReservation> _reservations;

        /// <summary>
        /// Initializes a new instance of QrVerificationService.
        /// </summary>
        public QrVerificationService(MongoDbContext context)
        {
            _reservations = context.Reservations;
        }

        /// <summary>
        /// Rule R9: Scanning verifies QrToken with the server.
        /// </summary>
        public async Task<EnergyReservation> VerifyQrAsync(QrVerifyRequest dto)
        {
            var reservation = await _reservations.Find(r => r.Id == dto.ReservationId).FirstOrDefaultAsync();
            if (reservation == null)
            {
                throw BusinessRuleException.NotFound("Reservation not found.");
            }

            if (reservation.Status != ReservationStatus.Approved)
            {
                throw BusinessRuleException.Conflict($"Reservation is not Approved. Current status: {reservation.Status}");
            }

            if (reservation.QrToken != dto.QrToken)
            {
                throw new BusinessRuleException("Invalid QR token.", StatusCodes.Status400BadRequest);
            }

            return reservation;
        }

        /// <summary>
        /// Rule R9: Completing the energy transfer sets Completed.
        /// </summary>
        public async Task<EnergyReservation> CompleteAsync(string reservationId, string operatorNic)
        {
            var reservation = await _reservations.Find(r => r.Id == reservationId).FirstOrDefaultAsync();
            if (reservation == null)
            {
                throw BusinessRuleException.NotFound("Reservation not found.");
            }

            if (reservation.Status != ReservationStatus.Approved)
            {
                throw BusinessRuleException.Conflict($"Reservation is not Approved. Cannot complete.");
            }

            reservation.Status = ReservationStatus.Completed;
            reservation.CompletedBy = operatorNic;
            reservation.CompletedAt = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _reservations.ReplaceOneAsync(r => r.Id == reservationId, reservation);
            return reservation;
        }
    }
}
