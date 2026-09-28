/*
 * File:        BusinessRuleException.cs
 * Author:      [Your Name] ([IT Number])
 * Description: Exception thrown by services when a business rule is broken
 *              (e.g. booking more than 7 days ahead). The error handling
 *              middleware turns it into an HTTP response with a message.
 * Created:     28/09/2026
 */

namespace SolarGrid.Api.Helpers
{
    /// <summary>
    /// Thrown when a request breaks a business rule. Carries the HTTP status code to return.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public int StatusCode { get; }

        /// <summary>
        /// Creates the exception with a user-friendly message and a status code (400 by default).
        /// </summary>
        public BusinessRuleException(string message, int statusCode = StatusCodes.Status400BadRequest)
            : base(message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Shortcut for a 404 Not Found error.
        /// </summary>
        public static BusinessRuleException NotFound(string message) =>
            new(message, StatusCodes.Status404NotFound);

        /// <summary>
        /// Shortcut for a 409 Conflict error, e.g. deleting something that is still in use.
        /// </summary>
        public static BusinessRuleException Conflict(string message) =>
            new(message, StatusCodes.Status409Conflict);

        /// <summary>
        /// Shortcut for a 403 Forbidden error, e.g. an account that is not active.
        /// </summary>
        public static BusinessRuleException Forbidden(string message) =>
            new(message, StatusCodes.Status403Forbidden);
    }
}
