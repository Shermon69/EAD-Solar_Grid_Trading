/*
 * File:        CreateReservationViewModel.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: View model for the web Create Reservation form, plus the
 *              prosumer, station and slot dropdown options it uses.
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Web.Models
{
    public class CreateReservationViewModel
    {
        // Stores the NIC of the prosumer selected for the reservation.
        [Required(ErrorMessage = "Please select a prosumer.")]
        public string ProsumerNic { get; set; } = string.Empty;

        // Contains the active prosumers available for selection in the form.
        public List<ProsumerOption> Prosumers { get; set; } = new();

        // Stores the identifier of the solar station selected for the reservation.
        [Required(ErrorMessage = "Please select a station.")]
        public string StationId { get; set; } = string.Empty;

        // Contains the active solar stations available for selection.
        public List<StationOption> Stations { get; set; } = new();

        // Stores the date selected for the reservation.
        [Required(ErrorMessage = "Please select a date.")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; } = DateTime.Today;

        // Stores the identifier of the time slot selected for the reservation.
        [Required(ErrorMessage = "Please select a time slot.")]
        public string SlotId { get; set; } = string.Empty;

        // Contains the available time slots for the selected station and date.
        public List<SlotOption> Slots { get; set; } = new();

        // Stores the type of energy reservation, defaulting to Charging.
        [Required(ErrorMessage = "Please select a reservation type.")]
        public string Type { get; set; } = "Charging";

        // Stores the amount of energy requested in kilowatt-hours.
        [Range(0.1, 1000, ErrorMessage = "Energy must be greater than 0.")]
        public double EnergyKwh { get; set; }
    }

    public class ProsumerOption
    {
        // Stores the NIC of the prosumer.
        public string Nic { get; set; } = string.Empty;

        // Stores the full name of the prosumer.
        public string FullName { get; set; } = string.Empty;

        // Stores the current status of the prosumer.
        public string Status { get; set; } = string.Empty;
    }

    public class StationOption
    {
        // Stores the unique identifier of the solar station.
        public string Id { get; set; } = string.Empty;

        // Stores the name of the solar station.
        public string Name { get; set; } = string.Empty;

        // Stores the address of the solar station.
        public string Address { get; set; } = string.Empty;
    }

    public class SlotOption
    {
        // Stores the unique identifier of the energy booking slot.
        public string Id { get; set; } = string.Empty;

        // Stores the identifier of the station associated with the slot.
        public string StationId { get; set; } = string.Empty;

        // Stores the starting time of the booking slot.
        public DateTime StartTime { get; set; }

        // Stores the ending time of the booking slot.
        public DateTime EndTime { get; set; }

        // Stores the total number of reservations supported by the slot.
        public int TotalSlots { get; set; }

        // Stores the number of remaining available reservations.
        public int AvailableSlots { get; set; }

        // Indicates whether the slot is currently available for booking.
        public bool IsAvailable { get; set; }
    }
}