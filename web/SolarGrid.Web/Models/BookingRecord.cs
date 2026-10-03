/*
 * File:        BookingRecord.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: View model used to display and edit energy booking records
 *              in the web application.
 * Created:     29/09/2026
 */

using System.Text.Json.Serialization;

namespace SolarGrid.Web.Models
{
    public class BookingRecord
    {
        // Stores the unique identifier of the reservation.
        public string Id { get; set; } = string.Empty;

        // Stores the NIC of the prosumer associated with the reservation.
        public string ProsumerNic { get; set; } = string.Empty;

        // Stores the identifier of the solar station selected for the reservation.
        public string StationId { get; set; } = string.Empty;

        // Stores the display name of the selected solar station.
        public string StationName { get; set; } = string.Empty;

        // Stores the identifier of the selected energy booking slot.
        public string SlotId { get; set; } = string.Empty;

        // Maps the reservationTime field returned by the API to the web model.
        [JsonPropertyName("reservationTime")]
        public DateTime ReservationDate { get; set; }

        // Stores the type of energy reservation.
        public string Type { get; set; } = string.Empty;

        // Stores the amount of energy requested in kilowatt-hours.
        public double EnergyKwh { get; set; }

        // Stores the current status of the reservation.
        public string Status { get; set; } = string.Empty;

        // Stores the QR token generated for an approved reservation, if available.
        public string? QrToken { get; set; }
    }
}