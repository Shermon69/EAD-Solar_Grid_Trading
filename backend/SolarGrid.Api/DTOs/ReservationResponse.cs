/*
 * File:        ReservationResponse.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Response model returned to clients for energy reservations.
 */

namespace SolarGrid.Api.DTOs
{
    /// <summary>
    /// Details returned to clients for an energy reservation.
    /// </summary>
    public class ReservationResponse
    {
        // Unique identifier of the reservation.
        public string Id { get; set; } = string.Empty;

        // NIC of the prosumer who made the reservation.
        public string ProsumerNic { get; set; } = string.Empty;

        // Identifier of the selected solar station.
        public string StationId { get; set; } = string.Empty;

        // Display name of the selected solar station.
        public string StationName { get; set; } = string.Empty;

        // Identifier of the selected energy booking slot.
        public string SlotId { get; set; } = string.Empty;

        // Date and time when the reservation is scheduled.
        public DateTime ReservationTime { get; set; }

        // Type of the energy reservation.
        public string Type { get; set; } = string.Empty;

        // Amount of energy associated with the reservation in kilowatt-hours.
        public double EnergyKwh { get; set; }

        // Current status of the reservation.
        public string Status { get; set; } = string.Empty;

        // QR token associated with the reservation when applicable.
        public string? QrToken { get; set; }

        // NIC or identifier of the person who completed the reservation.
        public string? CompletedBy { get; set; }

        // Date and time when the reservation was created.
        public DateTime CreatedAt { get; set; }

        // Date and time when the reservation was last updated.
        public DateTime UpdatedAt { get; set; }

        // Date and time when the reservation was completed, if applicable.
        public DateTime? CompletedAt { get; set; }
    }
}