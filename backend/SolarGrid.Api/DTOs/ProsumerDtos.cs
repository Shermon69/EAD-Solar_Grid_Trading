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
/// Request body for POST /api/prosumers/register. The password is hashed
/// with BCrypt in the service; it is never stored in plain text.
/// </summary>
public class RegisterRequest
{
    /// <summary>NIC in the format 200012345678 or 991234567V.</summary>
    [Required, RegularExpression(@"^([0-9]{9}[vVxX]|[0-9]{12})$", ErrorMessage = "Invalid NIC")]
    public string Nic { get; set; } = string.Empty;

    /// <summary>Full name of the prosumer (3 to 100 characters).</summary>
    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Contact email address.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Sri Lankan mobile number (10 digits, starts with 0).</summary>
    [Required, RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Phone must be 10 digits")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Home or business address (up to 200 characters).</summary>
    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    /// <summary>Plain password (minimum 8 characters), hashed before saving.</summary>
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request body for PUT /api/prosumers/{nic} and PUT /api/prosumers/me/profile.
/// The NIC cannot be changed; NewPassword is optional.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>Full name of the prosumer (3 to 100 characters).</summary>
    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Contact email address.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Sri Lankan mobile number (10 digits, starts with 0).</summary>
    [Required, RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Phone must be 10 digits")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Home or business address (up to 200 characters).</summary>
    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    /// <summary>Optional new password (minimum 8 characters). Null means keep the old one.</summary>
    [MinLength(8)]
    public string? NewPassword { get; set; }
}

/// <summary>
/// Response returned for a prosumer. Never contains PasswordHash.
/// </summary>
public class ProsumerResponse
{
    /// <summary>NIC, which is also the primary key (_id) in the Users collection.</summary>
    public string Nic { get; set; } = string.Empty;

    /// <summary>Full name of the prosumer.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Contact email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Contact phone number.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Home or business address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>One of: Pending, Active, Deactivated.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>True if the prosumer asked the Backoffice to deactivate the account.</summary>
    public bool DeactivationRequested { get; set; }

    /// <summary>UTC timestamp when the account was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Read-only view of a reservation for the prosumer's own booking lists.
/// The fields match Member 4's EnergyReservations collection.
/// </summary>
public class BookingResponse
{
    /// <summary>Booking reference (the reservation's _id).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Name of the station where the booking was made.</summary>
    public string StationName { get; set; } = string.Empty;

    /// <summary>Start time of the booked slot (UTC).</summary>
    public DateTime ReservationDate { get; set; }

    /// <summary>One of: Pending, Approved, Completed, Cancelled.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>QR token, only set when the booking is Approved.</summary>
    public string? QrToken { get; set; }
}

/// <summary>
/// Counts shown on the dashboard: pending bookings and approved future bookings.
/// </summary>
public class DashboardResponse
{
    /// <summary>Number of bookings with status Pending.</summary>
    public long PendingCount { get; set; }

    /// <summary>Number of approved bookings whose reservation time is in the future.</summary>
    public long ApprovedFutureCount { get; set; }
}