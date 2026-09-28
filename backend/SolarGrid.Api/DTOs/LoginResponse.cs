/*
 * File:        LoginResponse.cs
 * Author:      [Your Name] ([IT Number])
 * Description: Data returned to the client after a successful login. The
 *              client stores the token and sends it with every later request.
 * Created:     28/09/2026
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
