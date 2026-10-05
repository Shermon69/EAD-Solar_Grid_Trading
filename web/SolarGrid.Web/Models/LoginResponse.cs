/*
 * File:        LoginResponse.cs
 * Author:      Shermon H (IT22177964)
 * Description: Matches the JSON returned by POST /api/auth/login.
 */

namespace SolarGrid.Web.Models
{
    /// <summary>
    /// Token and user details returned by the API after login.
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Nic { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
