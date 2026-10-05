/*
 * File:        NicValidator.cs
 * Author:      Shermon H (IT22177964)
 * Description: Checks that a Sri Lankan NIC number has a valid format.
 *              Old format: 9 digits + V or X (e.g. 991234567V).
 *              New format: 12 digits (e.g. 199912345678).
 */

using System.Text.RegularExpressions;

namespace SolarGrid.Api.Helpers
{
    /// <summary>
    /// NIC format checks. Can be used by any service that accepts a NIC.
    /// </summary>
    public static class NicValidator
    {
        private static readonly Regex NicPattern = new(@"^(\d{9}[VX]|\d{12})$", RegexOptions.Compiled);

        /// <summary>
        /// Makes the NIC consistent before saving or searching: removes spaces and uses upper case.
        /// </summary>
        public static string Normalize(string nic) => (nic ?? string.Empty).Trim().ToUpper();

        /// <summary>
        /// Returns true if the NIC is in the old or new Sri Lankan format.
        /// </summary>
        public static bool IsValid(string nic) => NicPattern.IsMatch(Normalize(nic));
    }
}
