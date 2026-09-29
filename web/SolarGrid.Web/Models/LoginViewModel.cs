/*
 * File:        LoginViewModel.cs
 * Author:      Shermon H (IT22177964)
 * Description: Form data for the web login page.
 * Created:     28/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Web.Models
{
    /// <summary>
    /// NIC and password typed on the login page.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your NIC.")]
        [Display(Name = "NIC")]
        public string Nic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Page to go back to after login (if the user was sent to login from another page)
        public string? ReturnUrl { get; set; }
    }
}
