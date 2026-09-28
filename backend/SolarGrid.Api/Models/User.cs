/*
 * File:        User.cs
 * Author:      Shermon H (IT22177964)
 * Description: MongoDB document for the "Users" collection. Stores all three
 *              user types (Backoffice, GridOperator, Prosumer). The NIC is the
 *              primary key (_id).
 * Created:     28/09/2026
 */

using MongoDB.Bson.Serialization.Attributes;

namespace SolarGrid.Api.Models
{
    /// <summary>
    /// A system user. The NIC number is used as the MongoDB _id.
    /// </summary>
    public class User
    {
        [BsonId]
        public string Nic { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // BCrypt hash of the password (the plain password is never stored)
        public string PasswordHash { get; set; } = string.Empty;

        // One of the values in the Roles class
        public string Role { get; set; } = Roles.Prosumer;

        // One of the values in the UserStatus class
        public string Status { get; set; } = UserStatus.Pending;

        // Set to true when a prosumer asks for deactivation from the mobile app
        public bool DeactivationRequested { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
