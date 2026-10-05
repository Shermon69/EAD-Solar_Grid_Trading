/*
 * File:        Constants.cs
 * Author:      Shermon H (IT22177964)
 * Description: Fixed values used across the system for user roles,
 *              account statuses, reservation statuses and reservation types.
 *              Using constants avoids spelling mistakes in strings.
 */

namespace SolarGrid.Api.Models
{
    /// <summary>
    /// The three types of users in the system.
    /// </summary>
    public static class Roles
    {
        public const string Backoffice = "Backoffice";
        public const string GridOperator = "GridOperator";
        public const string Prosumer = "Prosumer";

        // Used in [Authorize(Roles = ...)] for endpoints that any staff member can use
        public const string Staff = Backoffice + "," + GridOperator;
    }

    /// <summary>
    /// Account statuses. New prosumers start as Pending until Backoffice activates them.
    /// </summary>
    public static class UserStatus
    {
        public const string Pending = "Pending";
        public const string Active = "Active";
        public const string Deactivated = "Deactivated";
    }

    /// <summary>
    /// The life cycle of an energy reservation.
    /// </summary>
    public static class ReservationStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    /// <summary>
    /// Charging = prosumer takes energy from the grid, Dropoff = prosumer sells energy to the grid.
    /// </summary>
    public static class ReservationType
    {
        public const string Charging = "Charging";
        public const string Dropoff = "Dropoff";
    }
}
