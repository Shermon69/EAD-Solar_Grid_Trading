/*
 * File:        EnergySlot.cs
 * Author:      Shermon H (IT22177964)
 * Description: MongoDB document for the "EnergyBookingSlots" collection.
 *              A time block at a station with a number of battery slots
 *              that prosumers can reserve.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SolarGrid.Api.Models
{
    /// <summary>
    /// A bookable time block at a solar station.
    /// </summary>
    public class EnergySlot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // References SolarStationInfo._id
        public string StationId { get; set; } = string.Empty;

        // Stored in UTC
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Battery slots offered in this time block
        public int TotalSlots { get; set; }

        // Slots still free. Decreases when a reservation is made, increases when one is cancelled
        public int AvailableSlots { get; set; }

        // Grid operators can switch a slot off without deleting it
        public bool IsAvailable { get; set; } = true;
    }
}
