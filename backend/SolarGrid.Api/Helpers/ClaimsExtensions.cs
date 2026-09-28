/*
 * File:        ClaimsExtensions.cs
 * Author:      [Your Name] ([IT Number])
 * Description: Small helper methods to read the logged-in user's NIC and role
 *              from the JWT token inside a controller (e.g. User.GetNic()).
 * Created:     28/09/2026
 */

using System.Security.Claims;

namespace SolarGrid.Api.Helpers
{
    /// <summary>
    /// Extension methods for reading values from the logged-in user's token.
    /// </summary>
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Returns the NIC of the logged-in user.
        /// </summary>
        public static string GetNic(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        /// <summary>
        /// Returns the role of the logged-in user (Backoffice, GridOperator or Prosumer).
        /// </summary>
        public static string GetRole(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
