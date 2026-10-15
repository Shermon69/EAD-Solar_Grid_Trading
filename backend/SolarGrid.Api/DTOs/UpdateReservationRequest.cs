/*
 * File:        UpdateReservationRequest.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Data sent by the client when updating an energy reservation.
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Api.DTOs
{
    /// <summary>
    /// Details that can be updated for an energy reservation.
    /// </summary>
    public class UpdateReservationRequest
    {
        // Stores the identifier of the solar station selected for the updated reservation.
        [Required(ErrorMessage = "Station ID is required.")]
        public string StationId { get; set; } = string.Empty;

        // Stores the identifier of the energy booking slot selected for the updated reservation.
        [Required(ErrorMessage = "Slot ID is required.")]
        public string SlotId { get; set; } = string.Empty;

        // Stores the updated date and time of the reservation.
        [Required(ErrorMessage = "Reservation time is required.")]
        public DateTime ReservationTime { get; set; }

        // Stores the updated type of the energy reservation.
        [Required(ErrorMessage = "Reservation type is required.")]
        public string Type { get; set; } = string.Empty;

        // Stores the updated amount of energy in kilowatt-hours.
        [Range(0.01, double.MaxValue, ErrorMessage = "Energy must be greater than 0.")]
        public double EnergyKwh { get; set; }
    }
}