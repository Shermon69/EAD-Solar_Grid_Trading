/*
 * File:        EnergyReservation.cs
 * Author:      [Your Name] ([IT Number])
 * Description: MongoDB document for the "EnergyReservations" collection.
 *              A prosumer's booking of an energy slot at a station, including
 *              its status and the QR token used by grid operators to verify it.
 * Created:     28/09/2026
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SolarGrid.Api.Models
{
    /// <summary>
    /// An energy trading reservation made by a prosumer.
    /// </summary>
    public class EnergyReservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // References Users._id
        public string ProsumerNic { get; set; } = string.Empty;

        // References SolarStationInfo._id
        public string StationId { get; set; } = string.Empty;

        // References EnergyBookingSlots._id
        public string SlotId { get; set; } = string.Empty;

        // Start time of the booked slot (UTC)
        public DateTime ReservationTime { get; set; }

        // One of the values in the ReservationType class
        public string Type { get; set; } = ReservationType.Charging;

        public double EnergyKwh { get; set; }

        // One of the values in the ReservationStatus class
        public string Status { get; set; } = ReservationStatus.Pending;

        // Random value created when the booking is approved. It goes inside the QR code
        public string? QrToken { get; set; }

        // NIC of the grid operator who scanned the QR code and completed the transfer
        public string? CompletedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
