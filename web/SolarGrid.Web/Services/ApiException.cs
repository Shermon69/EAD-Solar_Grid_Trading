/*
 * File:        ApiException.cs
 * Author:      Shermon H (IT22177964)
 * Description: Error thrown by ApiClient when the Web API returns an error or
 *              cannot be reached. The message is the one sent by the API, so
 *              it can be shown to the user directly.
 * Created:     28/09/2026
 */

namespace SolarGrid.Web.Services
{
    /// <summary>
    /// An error returned by the Web API.
    /// </summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        /// <summary>
        /// Creates the exception with the API's error message and HTTP status code.
        /// </summary>
        public ApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
