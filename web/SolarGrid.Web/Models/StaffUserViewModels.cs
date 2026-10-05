/*
 * File:        StaffUserViewModels.cs
 * Author:      Shermon H (IT22177964)
 * Description: Models for the Staff User Management pages. StaffUserViewModel
 *              matches the JSON from /api/users, and StaffUserFormViewModel holds
 *              the create/edit form fields.
 */

using System.ComponentModel.DataAnnotations;
using SolarGrid.Web.Helpers;

namespace SolarGrid.Web.Models
{
    /// <summary>
    /// A staff user as returned by the API.
    /// </summary>
    public class StaffUserViewModel
    {
        public string Nic { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // "GridOperator" is shown as "Grid Operator"
        public string RoleDisplay => Role == Roles.GridOperator ? "Grid Operator" : Role;
    }

    /// <summary>
    /// Fields on the Create / Edit staff user form.
    /// </summary>
    public class StaffUserFormViewModel
    {
        // True on the Edit page (NIC cannot be changed, password is optional)
        public bool IsEdit { get; set; }

        [Required(ErrorMessage = "NIC is required.")]
        [RegularExpression(@"^(\d{9}[VvXx]|\d{12})$", ErrorMessage = "Enter 9 digits followed by V/X, or 12 digits.")]
        [Display(Name = "NIC")]
        public string Nic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must be 10 digits and start with 0.")]
        public string Phone { get; set; } = string.Empty;

        public string? Address { get; set; }

        [Required(ErrorMessage = "Please choose a role.")]
        public string Role { get; set; } = Roles.GridOperator;

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string? ConfirmPassword { get; set; }
    }
}
