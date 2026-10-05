/*
 * File:        BookingRecord.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Read-only projection of a reservation in the EnergyReservations
 *              collection. Maps the actual MongoDB field names (ReservationTime,
 *              StationId) to display-friendly names for the prosumer booking
 *              lists and dashboard counts.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SolarGrid.Api.Models;

/// <summary>
/// Read-only view of a reservation, used by the prosumer's own booking lists
/// and dashboard counts. Maps the real MongoDB field names.
/// </summary>
[BsonIgnoreExtraElements]
public class BookingRecord
{
    // Booking reference (the reservation's _id).
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    // NIC of the prosumer who made the booking.
    [BsonElement("ProsumerNic")]
    public string ProsumerNic { get; set; } = string.Empty;

    // Station reference (ObjectId as string) - used to look up the name.
    [BsonElement("StationId")]
    public string StationId { get; set; } = string.Empty;

    // Station name is NOT stored in the reservation - it is filled in by
    // the service after looking up the SolarStationInfo collection.
    [BsonIgnore]
    public string StationName { get; set; } = string.Empty;

    // Start time of the booked slot (UTC). Stored as ReservationTime in MongoDB.
    [BsonElement("ReservationTime")]
    public DateTime ReservationDate { get; set; }

    // Reservation type: Charging or Dropoff.
    [BsonElement("Type")]
    public string Type { get; set; } = string.Empty;

    // Amount of energy involved in the reservation.
    [BsonElement("EnergyKwh")]
    public double EnergyKwh { get; set; }

    // One of: Pending, Approved, Completed, Cancelled.
    [BsonElement("Status")]
    public string Status { get; set; } = "Pending";

    // QR token, only set when the booking is Approved.
    [BsonElement("QrToken")]
    public string? QrToken { get; set; }
}