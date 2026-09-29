/*
 * File:        ErrorHandlingMiddleware.cs
 * Author:      Shermon H (IT22177964)
 * Description: Catches exceptions from any controller or service and returns
 *              them as JSON in the same format: { "message": "..." }.
 *              This way the web and mobile apps can always show the message.
 * Created:     28/09/2026
 */

using System.Text.Json;

namespace SolarGrid.Api.Helpers
{
    /// <summary>
    /// Middleware that converts exceptions into clean JSON error responses.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        /// <summary>
        /// Receives the next step in the request pipeline and a logger.
        /// </summary>
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Runs the request and catches any exception thrown while handling it.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessRuleException ex)
            {
                // Expected error, e.g. a broken business rule
                await WriteErrorAsync(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                // Unexpected error: log the details, but only send a general message to the client
                _logger.LogError(ex, "Unhandled error");
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError,
                    "Something went wrong on the server. Please try again.");
            }
        }

        /// <summary>
        /// Writes { "message": "..." } with the given status code.
        /// </summary>
        private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
        }
    }
}
