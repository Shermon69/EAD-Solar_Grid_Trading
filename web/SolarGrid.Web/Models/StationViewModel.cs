/*
 * File:        StationViewModel.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: View models for Solar Stations in the Web app.
 * Created:     29/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Web.Models
{
    public class StationViewModel
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double CapacityKw { get; set; }
        public int BatterySlots { get; set; }
        public bool IsActive { get; set; }
        public List<OperatingHoursViewModel> Schedule { get; set; } = new();
    }

    public class OperatingHoursViewModel
    {
        public string Day { get; set; } = string.Empty;
        public string OpenTime { get; set; } = "06:00";
        public string CloseTime { get; set; } = "18:00";
    }

    public class CreateStationViewModel
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string Address { get; set; } = string.Empty;
        [Required] public double Latitude { get; set; }
        [Required] public double Longitude { get; set; }
        [Required] public double CapacityKw { get; set; }
        [Required] public int BatterySlots { get; set; }
    }
}
