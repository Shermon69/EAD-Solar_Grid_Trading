/*
 * File:        StaffUserDtos.cs
 * Author:      Shermon H (IT22177964)
 * Description: Request and response objects for staff user management
 *              (Backoffice and Grid Operator accounts). The password hash is
 *              never sent back to clients.
 * Created:     29/09/2026
 */

using System.ComponentModel.DataAnnotations;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.DTOs
{
    /// <summary>
    /// Data needed to create a new staff user.
    /// </summary>
    public class CreateStaffUserRequest
    {
        [Required(ErrorMessage = "NIC is required.")]
        public string Nic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name can be at most 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must be 10 digits and start with 0.")]
        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = Roles.GridOperator;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data that can be changed on an existing staff user. NIC cannot be changed
    /// because it is the primary key. Leave NewPassword empty to keep the old password.
    /// </summary>
    public class UpdateStaffUserRequest
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name can be at most 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must be 10 digits and start with 0.")]
        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = Roles.GridOperator;

        [MinLength(6, ErrorMessage = "New password must be at least 6 characters.")]
        public string? NewPassword { get; set; }
    }

    /// <summary>
    /// Staff user details returned to clients (without the password hash).
    /// </summary>
    public class StaffUserResponse
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

        /// <summary>
        /// Copies the safe fields from a User document.
        /// </summary>
        public static StaffUserResponse FromUser(User user) => new()
        {
            Nic = user.Nic,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
