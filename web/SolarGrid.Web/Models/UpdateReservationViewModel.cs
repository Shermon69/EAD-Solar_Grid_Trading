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
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a prosumer.")]
        public string ProsumerNic { get; set; } = string.Empty;

        public List<ProsumerOption> Prosumers { get; set; } = new();

        [Required(ErrorMessage = "Please select a station.")]
        public string StationId { get; set; } = string.Empty;

        public List<StationOption> Stations { get; set; } = new();

        [Required(ErrorMessage = "Please select a date.")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Please select a time slot.")]
        public string SlotId { get; set; } = string.Empty;

        public List<SlotOption> Slots { get; set; } = new();

        [Required(ErrorMessage = "Please select a reservation type.")]
        public string Type { get; set; } = "Charging";

        [Range(0.1, 1000, ErrorMessage = "Energy must be greater than 0.")]
        public double EnergyKwh { get; set; }
    }
}