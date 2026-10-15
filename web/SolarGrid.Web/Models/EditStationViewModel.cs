/*
 * File:        EditStationViewModel.cs
 * Author:      Shermon H (IT22177964)
 * Description: Form model for the Edit Station page: station details plus a
 *              weekly operating schedule with one row per day.
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Web.Models
{
    /// <summary>
    /// Fields on the Edit Station form.
    /// </summary>
    public class EditStationViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Station name is required.")]
        [Display(Name = "Station name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double Longitude { get; set; }

        [Range(0.1, 100000, ErrorMessage = "Capacity must be greater than 0.")]
        [Display(Name = "Capacity (kW/h)")]
        public double CapacityKw { get; set; }

        [Range(1, 1000, ErrorMessage = "Battery slots must be at least 1.")]
        [Display(Name = "Battery storage slots")]
        public int BatterySlots { get; set; }

        public bool IsActive { get; set; }

        // Always 7 rows (Monday to Sunday)
        public List<ScheduleDayViewModel> Schedule { get; set; } = new();

        /// <summary>
        /// Builds the 7 schedule rows from the station's saved schedule.
        /// Days that are not in the saved schedule are shown as closed.
        /// </summary>
        public static List<ScheduleDayViewModel> BuildWeek(List<OperatingHoursViewModel> saved)
        {
            var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            return days.Select(day =>
            {
                var match = saved.FirstOrDefault(s => s.Day.Equals(day, StringComparison.OrdinalIgnoreCase));
                return new ScheduleDayViewModel
                {
                    Day = day,
                    IsOpen = match != null,
                    OpenTime = match?.OpenTime ?? "06:00",
                    CloseTime = match?.CloseTime ?? "18:00"
                };
            }).ToList();
        }
    }

    /// <summary>
    /// One day in the Edit Station schedule table.
    /// </summary>
    public class ScheduleDayViewModel
    {
        public string Day { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public string OpenTime { get; set; } = "06:00";
        public string CloseTime { get; set; } = "18:00";
    }
}
