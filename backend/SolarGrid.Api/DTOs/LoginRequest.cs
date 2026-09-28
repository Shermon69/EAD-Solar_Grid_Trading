/*
 * File:        LoginRequest.cs
 * Author:      Shermon H (IT22177964)
 * Description: Data sent by the web or mobile app when a user logs in.
 * Created:     28/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Api.DTOs
{
    /// <summary>
    /// Login details: NIC and password.
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "NIC is required.")]
        public string Nic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
