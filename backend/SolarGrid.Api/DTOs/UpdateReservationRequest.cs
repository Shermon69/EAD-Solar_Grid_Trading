/*
 * File:        UpdateReservationRequest.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Data sent by the client when updating an energy reservation.
 * Created:     29/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Api.DTOs
{
    /// <summary>
    /// Details that can be updated for an energy reservation.
    /// </summary>
    public class UpdateReservationRequest
    {
        [Required(ErrorMessage = "Station ID is required.")]
        public string StationId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slot ID is required.")]
        public string SlotId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reservation time is required.")]
        public DateTime ReservationTime { get; set; }

        [Required(ErrorMessage = "Reservation type is required.")]
        public string Type { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Energy must be greater than 0.")]
        public double EnergyKwh { get; set; }
    }
}