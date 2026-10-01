/*
 * File:        SlotViewModel.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: View models for Energy Slots in the Web app.
 * Created:     29/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Web.Models
{
    public class SlotViewModel
    {
        public string? Id { get; set; }
        public string StationId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateSlotViewModel
    {
        [Required] public string StationId { get; set; } = string.Empty;
        [Required] public DateTime StartTime { get; set; }
        [Required] public DateTime EndTime { get; set; }
        [Required] public int TotalSlots { get; set; }
    }
}
