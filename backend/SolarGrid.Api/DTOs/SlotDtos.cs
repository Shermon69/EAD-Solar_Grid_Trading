/*
 * File:        SlotDtos.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: DTOs for energy booking slots.
 */
using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Api.DTOs
{
    public class CreateSlotRequest
    {
        [Required] public string StationId { get; set; } = string.Empty;
        [Required] public DateTime StartTime { get; set; }
        [Required] public DateTime EndTime { get; set; }
        [Required] public int TotalSlots { get; set; }
    }

    public class UpdateSlotRequest
    {
        [Required] public int TotalSlots { get; set; }
        [Required] public bool IsAvailable { get; set; }
    }
}
