/*
 * File:        ErrorViewModel.cs
 * Author:      Shermon H (IT22177964)
 * Description: Data for the general error page (from the MVC project template).
 */

namespace SolarGrid.Web.Models
{
    /// <summary>
    /// Holds the request ID shown on the error page.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
