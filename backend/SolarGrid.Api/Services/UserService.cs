/*
 * File:        UserService.cs
 * Author:      Shermon H (IT22177964)
 * Description: Business logic for managing staff users (Backoffice and Grid
 *              Operator accounts): list, view, create, update, activate and
 *              deactivate. Prosumers are managed separately by ProsumerService.
 */

using MongoDB.Driver;
using SolarGrid.Api.Data;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services
{
    /// <summary>
    /// Staff user management rules.
    /// </summary>
    public class UserService
    {
        private static readonly string[] StaffRoles = { Roles.Backoffice, Roles.GridOperator };

        private readonly MongoDbContext _db;

        /// <summary>
        /// Receives the database context through dependency injection.
        /// </summary>
        public UserService(MongoDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns all staff users, optionally only one role, sorted by name.
        /// </summary>
        public async Task<List<StaffUserResponse>> GetStaffAsync(string? role)
        {
            if (!string.IsNullOrWhiteSpace(role) && !StaffRoles.Contains(role))
                throw new BusinessRuleException("Role must be Backoffice or GridOperator.");

            var filter = string.IsNullOrWhiteSpace(role)
                ? Builders<User>.Filter.In(u => u.Role, StaffRoles)
                : Builders<User>.Filter.Eq(u => u.Role, role);

            var users = await _db.Users.Find(filter).SortBy(u => u.FullName).ToListAsync();
            return users.Select(StaffUserResponse.FromUser).ToList();
        }

        /// <summary>
        /// Returns one staff user by NIC. Throws 404 if not found or if the NIC belongs to a prosumer.
        /// </summary>
        public async Task<StaffUserResponse> GetByNicAsync(string nic)
        {
            var user = await FindStaffAsync(nic);
            return StaffUserResponse.FromUser(user);
        }

        /// <summary>
        /// Creates a new Backoffice or Grid Operator account. The account is active immediately.
        /// Rules: valid NIC format, NIC not already used, role must be a staff role.
        /// </summary>
        public async Task<StaffUserResponse> CreateAsync(CreateStaffUserRequest request)
        {
            var nic = NicValidator.Normalize(request.Nic);

            if (!NicValidator.IsValid(nic))
                throw new BusinessRuleException("Invalid NIC. Use 9 digits followed by V/X, or 12 digits.");

            ValidateStaffRole(request.Role);

            if (await _db.Users.Find(u => u.Nic == nic).AnyAsync())
                throw BusinessRuleException.Conflict($"A user with NIC {nic} already exists.");

            await EnsureEmailNotUsedAsync(request.Email, nic);

            var user = new User
            {
                Nic = nic,
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLower(),
                Phone = request.Phone.Trim(),
                Address = request.Address.Trim(),
                Role = request.Role,
                Status = UserStatus.Active,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.Users.InsertOneAsync(user);
            return StaffUserResponse.FromUser(user);
        }

        /// <summary>
        /// Updates a staff user's details, role and (optionally) password.
        /// A user cannot change their own role, so there is always at least one Backoffice user.
        /// </summary>
        public async Task<StaffUserResponse> UpdateAsync(string nic, UpdateStaffUserRequest request, string currentUserNic)
        {
            var user = await FindStaffAsync(nic);

            ValidateStaffRole(request.Role);

            if (user.Nic == currentUserNic && request.Role != user.Role)
                throw new BusinessRuleException("You cannot change your own role.");

            await EnsureEmailNotUsedAsync(request.Email, user.Nic);

            user.FullName = request.FullName.Trim();
            user.Email = request.Email.Trim().ToLower();
            user.Phone = request.Phone.Trim();
            user.Address = request.Address.Trim();
            user.Role = request.Role;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _db.Users.ReplaceOneAsync(u => u.Nic == user.Nic, user);
            return StaffUserResponse.FromUser(user);
        }

        /// <summary>
        /// Activates a staff account so the user can log in again.
        /// </summary>
        public async Task ActivateAsync(string nic)
        {
            var user = await FindStaffAsync(nic);

            if (user.Status == UserStatus.Active)
                throw new BusinessRuleException("This user is already active.");

            await SetStatusAsync(user.Nic, UserStatus.Active);
        }

        /// <summary>
        /// Deactivates a staff account so the user can no longer log in.
        /// A Backoffice user cannot deactivate their own account.
        /// </summary>
        public async Task DeactivateAsync(string nic, string currentUserNic)
        {
            var user = await FindStaffAsync(nic);

            if (user.Nic == currentUserNic)
                throw new BusinessRuleException("You cannot deactivate your own account.");

            if (user.Status == UserStatus.Deactivated)
                throw new BusinessRuleException("This user is already deactivated.");

            await SetStatusAsync(user.Nic, UserStatus.Deactivated);
        }

        /// <summary>
        /// Finds a staff user by NIC or throws 404. Prosumer NICs are treated as not found.
        /// </summary>
        private async Task<User> FindStaffAsync(string nic)
        {
            var normalized = NicValidator.Normalize(nic);
            var user = await _db.Users.Find(u => u.Nic == normalized).FirstOrDefaultAsync();

            if (user == null || !StaffRoles.Contains(user.Role))
                throw BusinessRuleException.NotFound($"Staff user with NIC {normalized} was not found.");

            return user;
        }

        /// <summary>
        /// Throws if the role is not Backoffice or GridOperator.
        /// </summary>
        private static void ValidateStaffRole(string role)
        {
            if (!StaffRoles.Contains(role))
                throw new BusinessRuleException("Role must be Backoffice or GridOperator.");
        }

        /// <summary>
        /// Throws 409 if another user (different NIC) already uses this email.
        /// </summary>
        private async Task EnsureEmailNotUsedAsync(string email, string ownNic)
        {
            var normalized = email.Trim().ToLower();
            var taken = await _db.Users.Find(u => u.Email == normalized && u.Nic != ownNic).AnyAsync();

            if (taken)
                throw BusinessRuleException.Conflict("This email address is already used by another user.");
        }

        /// <summary>
        /// Saves a new account status and the update time.
        /// </summary>
        private async Task SetStatusAsync(string nic, string status)
        {
            var update = Builders<User>.Update
                .Set(u => u.Status, status)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            await _db.Users.UpdateOneAsync(u => u.Nic == nic, update);
        }
    }
}
