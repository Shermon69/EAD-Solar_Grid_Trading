using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Web.Models
{
    public class CreateReservationViewModel
    {
        [Required(ErrorMessage = "Please select a prosumer.")]
        public string ProsumerNic { get; set; } = string.Empty;

        public List<ProsumerOption> Prosumers { get; set; } = new();

        [Required(ErrorMessage = "Please select a station.")]
        public string StationId { get; set; } = string.Empty;

        public List<StationOption> Stations { get; set; } = new();

        [Required(ErrorMessage = "Please select a date.")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select a time slot.")]
        public string SlotId { get; set; } = string.Empty;

        public List<SlotOption> Slots { get; set; } = new();

        [Required(ErrorMessage = "Please select a reservation type.")]
        public string Type { get; set; } = "Charging";

        [Range(0.1, 1000, ErrorMessage = "Energy must be greater than 0.")]
        public double EnergyKwh { get; set; }
    }

    public class ProsumerOption
    {
        public string Nic { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class StationOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class SlotOption
    {
        public string Id { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsAvailable { get; set; }
    }
}