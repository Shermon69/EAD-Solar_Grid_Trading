/*
 * File:        AuthService.cs
 * Author:      [Your Name] ([IT Number])
 * Description: Business logic for logging in. Checks the NIC and password,
 *              makes sure the account is active, and creates a JWT token.
 * Created:     28/09/2026
 */

using MongoDB.Driver;
using SolarGrid.Api.Data;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services
{
    /// <summary>
    /// Handles user login.
    /// </summary>
    public class AuthService
    {
        private readonly MongoDbContext _db;
        private readonly JwtTokenGenerator _tokenGenerator;

        /// <summary>
        /// Receives the database context and the token generator.
        /// </summary>
        public AuthService(MongoDbContext db, JwtTokenGenerator tokenGenerator)
        {
            _db = db;
            _tokenGenerator = tokenGenerator;
        }

        /// <summary>
        /// Logs a user in. Throws an error if the NIC/password is wrong or the
        /// account is pending or deactivated. Returns a token if successful.
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var nic = request.Nic.Trim().ToUpper();
            var user = await _db.Users.Find(u => u.Nic == nic).FirstOrDefaultAsync();

            // Same message for a wrong NIC and a wrong password, so we don't reveal which NICs exist
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new BusinessRuleException("Invalid NIC or password.", StatusCodes.Status401Unauthorized);

            if (user.Status == UserStatus.Pending)
                throw BusinessRuleException.Forbidden("Your account is pending activation by the Backoffice.");

            if (user.Status == UserStatus.Deactivated)
                throw BusinessRuleException.Forbidden("Your account is deactivated. Please contact the Backoffice.");

            return new LoginResponse
            {
                Token = _tokenGenerator.GenerateToken(user),
                Nic = user.Nic,
                FullName = user.FullName,
                Role = user.Role
            };
        }
    }
}
