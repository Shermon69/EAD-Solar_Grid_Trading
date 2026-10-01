/*
 * File:        ProsumerViewModels.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: View models used by the Prosumer Management, Pending
 *              Activations and Dashboard pages.
 * Created:     29/09/2026
 */
using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Web.Models;

/// <summary>
/// Read-only view of a prosumer, used in the list and pending pages.
/// </summary>
public class ProsumerVm
{
    // NIC — also the primary key of the prosumer.
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

    // True if the prosumer asked Backoffice to deactivate the account.
    public bool DeactivationRequested { get; set; }

    // UTC timestamp when the account was created.
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Form model used on the Edit page. NIC is read-only; the rest are validated.
/// </summary>
public class ProsumerEditVm
{
    // NIC — read-only, cannot be changed.
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
}

/// <summary>
/// Dashboard view model — pending prosumer count and approved future
/// reservation count, read from the API.
/// </summary>
public class DashboardVm
{
    // Number of prosumers with status Pending.
    public long PendingCount { get; set; }

    // Number of approved bookings whose reservation time is in the future.
    public long ApprovedFutureCount { get; set; }
}