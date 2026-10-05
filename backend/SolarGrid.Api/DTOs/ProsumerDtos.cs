/*
 * File:        ProsumerDtos.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Request/response objects for the prosumer endpoints, with
 *              validation rules. The API validates these DTOs before the
 *              service is called.
 */

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SolarGrid.Api.Dtos;

/// <summary>
/// Request body for POST /api/prosumers/register. Password is hashed
/// with BCrypt in the service; it is never stored in plain text.
/// </summary>
public class RegisterRequest
{
    [Required, RegularExpression(@"^([0-9]{9}[vVxX]|[0-9]{12})$", ErrorMessage = "Invalid NIC")]
    public string Nic { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Phone must be 10 digits")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request body for updating a prosumer profile.
/// </summary>
public class UpdateProfileRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Phone must be 10 digits")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [MinLength(8)]
    public string? NewPassword { get; set; }
}

/// <summary>
/// Response returned for a prosumer. Never contains PasswordHash.
/// </summary>
public class ProsumerResponse
{
    public string Nic { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool DeactivationRequested { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Read-only view of a reservation for the prosumer's own booking lists.
/// Fields match Member 4's EnergyReservations collection.
/// </summary>
public class BookingResponse
{
    // Booking reference.
    public string Id { get; set; } = string.Empty;

    // Name of the station where the booking was made.
    public string StationName { get; set; } = string.Empty;

    // Start time of the booked slot in UTC.
    // Exposed as reservationTime so the Android client
    // can deserialize the same field used by Member 4's API.
    [JsonPropertyName("reservationTime")]
    public DateTime ReservationDate { get; set; }

    // Reservation type, such as Charging or Discharging.
    public string Type { get; set; } = string.Empty;

    // Amount of energy requested for the reservation.
    public double EnergyKwh { get; set; }

    // One of: Pending, Approved, Completed, Cancelled.
    public string Status { get; set; } = string.Empty;

    // QR token, only set when the booking is Approved.
    public string? QrToken { get; set; }
}

/// <summary>
/// Counts shown on the dashboard: pending bookings and approved future bookings.
/// </summary>
public class DashboardResponse
{
    public long PendingCount { get; set; }

    public long ApprovedFutureCount { get; set; }
}