/*
 * File:        BookingRecord.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Read-only projection of a reservation in the EnergyReservations
 *              collection. Used by the prosumer booking lists and dashboard
 *              counts. Extra fields in the document are ignored.
 * Created:     29/09/2026
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SolarGrid.Api.Models;

/// <summary>
/// Read-only view of a reservation, used by the prosumer's own booking lists
/// and dashboard counts.
/// </summary>
[BsonIgnoreExtraElements]
public class BookingRecord
{
    // Booking reference (the reservation's _id).
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    // NIC of the prosumer who made the booking.
    public string ProsumerNic { get; set; } = string.Empty;

    // Name of the station where the booking was made.
    public string StationName { get; set; } = string.Empty;

    // Start time of the booked slot (UTC).
    public DateTime ReservationDate { get; set; }

    // One of: Pending, Approved, Completed, Cancelled.
    public string Status { get; set; } = "Pending";

    // QR token, only set when the booking is Approved.
    public string? QrToken { get; set; }
}