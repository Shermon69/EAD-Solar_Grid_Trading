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
        public string Id { get; set; } = string.Empty;
        public string ProsumerNic { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;

        [JsonPropertyName("reservationTime")]
        public DateTime ReservationDate { get; set; }

        public string Type { get; set; } = string.Empty;
        public double EnergyKwh { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? QrToken { get; set; }
    }
}