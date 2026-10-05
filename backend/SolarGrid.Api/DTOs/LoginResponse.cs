/*
 * File:        LoginResponse.cs
 * Author:      Shermon H (IT22177964)
 * Description: Data returned to the client after a successful login. The
 *              client stores the token and sends it with every later request.
 */

namespace SolarGrid.Api.DTOs
{
    /// <summary>
    /// Token and basic user details returned after login.
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Nic { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
