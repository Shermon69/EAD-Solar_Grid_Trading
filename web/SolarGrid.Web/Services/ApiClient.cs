/*
 * File:        ApiClient.cs
 * Author:      Shermon H (IT22177964)
 * Description: The only class in the web app that talks to the Web API.
 *              It adds the logged-in user's JWT token to every request,
 *              converts JSON to C# objects and turns API errors into
 *              ApiException. The web app never connects to MongoDB directly.
 */

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SolarGrid.Web.Helpers;

namespace SolarGrid.Web.Services
{
    /// <summary>
    /// Wrapper around HttpClient for calling the Web API.
    /// Usage in a controller: var stations = await _api.GetAsync&lt;List&lt;StationViewModel&gt;&gt;("stations");
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // The API sends camelCase JSON; these options map it to PascalCase C# properties
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Receives the HttpClient (base URL set in Program.cs) and access to the current user.
        /// </summary>
        public ApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Sends a GET request and returns the response body as T.
        /// </summary>
        public async Task<T> GetAsync<T>(string url)
        {
            var response = await SendAsync(HttpMethod.Get, url);
            return await ReadAsync<T>(response);
        }

        /// <summary>
        /// Sends a POST request with a JSON body and returns the response body as T.
        /// </summary>
        public async Task<T> PostAsync<T>(string url, object? body = null)
        {
            var response = await SendAsync(HttpMethod.Post, url, body);
            return await ReadAsync<T>(response);
        }

        /// <summary>
        /// Sends a POST request when the response body is not needed.
        /// </summary>
        public async Task PostAsync(string url, object? body = null)
        {
            await SendAsync(HttpMethod.Post, url, body);
        }

        /// <summary>
        /// Sends a PUT request with a JSON body (used for updates).
        /// </summary>
        public async Task PutAsync(string url, object body)
        {
            await SendAsync(HttpMethod.Put, url, body);
        }

        /// <summary>
        /// Sends a PATCH request (used for actions like activate, deactivate, cancel, approve).
        /// </summary>
        public async Task PatchAsync(string url, object? body = null)
        {
            await SendAsync(HttpMethod.Patch, url, body);
        }

        /// <summary>
        /// Sends a DELETE request.
        /// </summary>
        public async Task DeleteAsync(string url)
        {
            await SendAsync(HttpMethod.Delete, url);
        }

        /// <summary>
        /// Builds and sends the request with the JWT token. Throws ApiException if the
        /// API returns an error or cannot be reached.
        /// </summary>
        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
        {
            var request = new HttpRequestMessage(method, url);

            // Add the token saved in the login cookie (if the user is logged in)
            var token = _httpContextAccessor.HttpContext?.User.FindFirst(AppClaims.AccessToken)?.Value;
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body != null)
                request.Content = JsonContent.Create(body, options: JsonOptions);

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                throw new ApiException("Cannot connect to the server. Please make sure the Web API is running.", 503);
            }

            if (!response.IsSuccessStatusCode)
                throw new ApiException(await ReadErrorMessageAsync(response), (int)response.StatusCode);

            return response;
        }

        /// <summary>
        /// Converts the JSON response body into an object of type T.
        /// </summary>
        private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
        {
            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return result!;
        }

        /// <summary>
        /// Reads the { "message": "..." } error sent by the API, or returns a general message.
        /// </summary>
        private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // A 401 on login means wrong NIC/password, otherwise the session has expired
                var loginError = await TryReadMessageAsync(response);
                return loginError ?? "Your session has expired. Please log in again.";
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return await TryReadMessageAsync(response) ?? "You do not have permission to do this.";

            return await TryReadMessageAsync(response) ?? $"Request failed ({(int)response.StatusCode}).";
        }

        /// <summary>
        /// Tries to read the "message" field from an error response. Returns null if there is none.
        /// </summary>
        private static async Task<string?> TryReadMessageAsync(HttpResponseMessage response)
        {
            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
                return string.IsNullOrWhiteSpace(error?.Message) ? null : error.Message;
            }
            catch
            {
                return null;
            }
        }

        // Shape of the error JSON returned by the API
        private class ApiError
        {
            public string? Message { get; set; }
        }
    }
}
