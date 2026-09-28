/*
 * File:        SolarStation.cs
 * Author:      Shermon H (IT22177964)
 * Description: MongoDB document for the "SolarStationInfo" collection.
 *              Represents a solar microgrid node (hub) with its GPS location,
 *              capacity, battery slots and weekly operating schedule.
 * Created:     28/09/2026
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SolarGrid.Api.Models
{
    /// <summary>
    /// A solar microgrid node (hub).
    /// </summary>
    public class SolarStation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // GPS location, used to show the station on Google Maps
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Capacity spec in kW/h
        public double CapacityKw { get; set; }

        // Total number of battery storage slots at this station
        public int BatterySlots { get; set; }

        public List<OperatingHours> Schedule { get; set; } = new();

        // NICs of the grid operators assigned to this station (references Users._id)
        public List<string> OperatorNics { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Opening hours of a station for one day of the week, e.g. Monday 06:00 - 18:00.
    /// </summary>
    public class OperatingHours
    {
        public string Day { get; set; } = string.Empty;
        public string OpenTime { get; set; } = "06:00";
        public string CloseTime { get; set; } = "18:00";
    }
}
