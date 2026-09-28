/*
 * File:        JwtTokenGenerator.cs
 * Author:      Shermon H (IT22177964)
 * Description: Creates a signed JWT token after a successful login. The token
 *              contains the user's NIC, name and role, so the API knows who is
 *              calling and what they are allowed to do.
 * Created:     28/09/2026
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Helpers
{
    /// <summary>
    /// Builds JWT tokens for logged-in users.
    /// </summary>
    public class JwtTokenGenerator
    {
        private readonly JwtSettings _settings;

        /// <summary>
        /// Receives the JWT settings from appsettings.json.
        /// </summary>
        public JwtTokenGenerator(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        /// <summary>
        /// Creates a token for the given user that expires after the configured number of hours.
        /// </summary>
        public string GenerateToken(User user)
        {
            // Claims = the information stored inside the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Nic),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_settings.ExpiryHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
