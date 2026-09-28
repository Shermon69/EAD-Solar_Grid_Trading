/*
 * File:        AppConstants.cs
 * Author:      Shermon H (IT22177964)
 * Description: Fixed values used in the web app: user role names (must match
 *              the API) and the custom claim that stores the API token.
 * Created:     28/09/2026
 */

namespace SolarGrid.Web.Helpers
{
    /// <summary>
    /// Role names used in [Authorize(Roles = ...)] and in the navbar. Must match the API.
    /// </summary>
    public static class Roles
    {
        public const string Backoffice = "Backoffice";
        public const string GridOperator = "GridOperator";
        public const string Prosumer = "Prosumer";

        // Any staff member (Backoffice or Grid Operator)
        public const string Staff = Backoffice + "," + GridOperator;
    }

    /// <summary>
    /// Custom claim names stored in the login cookie.
    /// </summary>
    public static class AppClaims
    {
        // The JWT token returned by the API. ApiClient sends it with every request
        public const string AccessToken = "access_token";
    }
}
