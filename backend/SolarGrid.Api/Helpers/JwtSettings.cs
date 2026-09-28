/*
 * File:        JwtSettings.cs
 * Author:      [Your Name] ([IT Number])
 * Description: Holds the JWT (login token) settings read from appsettings.json.
 * Created:     28/09/2026
 */

namespace SolarGrid.Api.Helpers
{
    /// <summary>
    /// Maps to the "JwtSettings" section of appsettings.json.
    /// </summary>
    public class JwtSettings
    {
        // Secret key used to sign tokens (at least 32 characters)
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryHours { get; set; } = 8;
    }
}
