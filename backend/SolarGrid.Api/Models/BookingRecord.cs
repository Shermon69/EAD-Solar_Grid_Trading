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
    /// <summary>Booking reference (the reservation's _id).</summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>NIC of the prosumer who made the booking.</summary>
    public string ProsumerNic { get; set; } = string.Empty;

    /// <summary>Name of the station where the booking was made.</summary>
    public string StationName { get; set; } = string.Empty;

    /// <summary>Start time of the booked slot (UTC).</summary>
    public DateTime ReservationDate { get; set; }

    /// <summary>One of: Pending, Approved, Completed, Cancelled.</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>QR token, only set when the booking is Approved.</summary>
    public string? QrToken { get; set; }
}