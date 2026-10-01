/*
 * File:        StationDtos.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: DTOs for station creation and updates.
 * Created:     29/09/2026
 */
using System.ComponentModel.DataAnnotations;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.DTOs
{
    public class CreateStationRequest
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string Address { get; set; } = string.Empty;
        [Required] public double Latitude { get; set; }
        [Required] public double Longitude { get; set; }
        [Required] public double CapacityKw { get; set; }
        [Required] public int BatterySlots { get; set; }
        public List<OperatingHours> Schedule { get; set; } = new();
    }

    public class UpdateStationRequest : CreateStationRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
