/*
 * File:        UpdateReservationViewModel.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: View model for updating an energy reservation.
 * Created:     29/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Web.Models
{
    public class UpdateReservationViewModel
    {
        // Stores the unique identifier of the reservation being updated.
        public string Id { get; set; } = string.Empty;

        // Stores the NIC of the prosumer associated with the reservation.
        [Required(ErrorMessage = "Please select a prosumer.")]
        public string ProsumerNic { get; set; } = string.Empty;

        // Contains the active prosumers available for selection.
        public List<ProsumerOption> Prosumers { get; set; } = new();

        // Stores the identifier of the solar station selected for the reservation.
        [Required(ErrorMessage = "Please select a station.")]
        public string StationId { get; set; } = string.Empty;

        // Contains the active solar stations available for selection.
        public List<StationOption> Stations { get; set; } = new();

        // Stores the updated reservation date.
        [Required(ErrorMessage = "Please select a date.")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        // Stores the identifier of the time slot selected for the updated reservation.
        [Required(ErrorMessage = "Please select a time slot.")]
        public string SlotId { get; set; } = string.Empty;

        // Contains the available time slots for the selected station and date.
        public List<SlotOption> Slots { get; set; } = new();

        // Stores the updated type of energy reservation.
        [Required(ErrorMessage = "Please select a reservation type.")]
        public string Type { get; set; } = "Charging";

        // Stores the updated amount of energy requested in kilowatt-hours.
        [Range(0.1, 1000, ErrorMessage = "Energy must be greater than 0.")]
        public double EnergyKwh { get; set; }
    }
}