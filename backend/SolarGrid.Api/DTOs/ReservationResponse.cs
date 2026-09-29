/*
 * File:        ReservationResponse.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Response model returned to clients for energy reservations.
 * Created:     29/09/2026
 */

namespace SolarGrid.Api.DTOs
{
    /// <summary>
    /// Details returned to clients for an energy reservation.
    /// </summary>
    public class ReservationResponse
    {
        public string Id { get; set; } = string.Empty;

        public string ProsumerNic { get; set; } = string.Empty;

        public string StationId { get; set; } = string.Empty;

        public string StationName { get; set; } = string.Empty;

        public string SlotId { get; set; } = string.Empty;

        public DateTime ReservationTime { get; set; }

        public string Type { get; set; } = string.Empty;

        public double EnergyKwh { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? QrToken { get; set; }

        public string? CompletedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}