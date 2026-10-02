/*
 * File:        ProsumerDtos.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Request/response objects for the prosumer endpoints, with
 *              validation rules. The API validates these DTOs before the
 *              service is called.
 * Created:     29/09/2026
 */
using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Api.Dtos;

/// <summary>
/// Request body for POST /api/prosumers/register. Password is hashed
/// with BCrypt in the service; it is never stored in plain text.
/// </summary>
public class RegisterRequest
{
    // NIC in the format 200012345678 or 991234567V.
    [Required, RegularExpression(@"^([0-9]{9}[vVxX]|[0-9]{12})$", ErrorMessage = "Invalid NIC")]
    public string Nic { get; set; } = string.Empty;

    // Full name of the prosumer (3 to 100 characters).
    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    // Contact email address.
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Sri Lankan mobile number (10 digits, starts with 0).
    [Required, RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Phone must be 10 digits")]
    public string Phone { get; set; } = string.Empty;

    // Home or business address (up to 200 characters).
    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    // Plain password (minimum 8 characters), hashed before saving.
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request body for PUT /api/prosumers/{nic} and PUT /api/prosumers/me/profile.
/// NIC cannot be changed; NewPassword is optional.
/// </summary>
public class UpdateProfileRequest
{
    // Full name of the prosumer (3 to 100 characters).
    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    // Contact email address.
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Sri Lankan mobile number (10 digits, starts with 0).
    [Required, RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Phone must be 10 digits")]
    public string Phone { get; set; } = string.Empty;

    // Home or business address (up to 200 characters).
    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    // Optional new password (minimum 8 characters). Null keeps the old one.
    [MinLength(8)]
    public string? NewPassword { get; set; }
}

/// <summary>
/// Response returned for a prosumer. Never contains PasswordHash.
/// </summary>
public class ProsumerResponse
{
    // NIC, which is also the primary key (_id) in the Users collection.
    public string Nic { get; set; } = string.Empty;

    // Full name of the prosumer.
    public string FullName { get; set; } = string.Empty;

    // Contact email address.
    public string Email { get; set; } = string.Empty;

    // Contact phone number.
    public string Phone { get; set; } = string.Empty;

    // Home or business address.
    public string Address { get; set; } = string.Empty;

    // One of: Pending, Active, Deactivated.
    public string Status { get; set; } = string.Empty;

    // True if the prosumer asked the Backoffice to deactivate the account.
    public bool DeactivationRequested { get; set; }

    // UTC timestamp when the account was created.
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Read-only view of a reservation for the prosumer's own booking lists.
/// Fields match Member 4's EnergyReservations collection.
/// </summary>
public class BookingResponse
{
    // Booking reference (the reservation's _id).
    public string Id { get; set; } = string.Empty;

    // Name of the station where the booking was made.
    public string StationName { get; set; } = string.Empty;

    // Start time of the booked slot (UTC).
    public DateTime ReservationDate { get; set; }

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
    // Number of bookings with status Pending.
    public long PendingCount { get; set; }

    // Number of approved bookings whose reservation time is in the future.
    public long ApprovedFutureCount { get; set; }
}