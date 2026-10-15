/*
 * File:        SessionExpiredFilter.cs
 * Author:      Shermon H (IT22177964)
 * Description: If any page gets a 401 (token expired) from the API, this filter
 *              logs the user out and sends them back to the login page, so
 *              individual controllers don't need to handle it.
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SolarGrid.Web.Services;

namespace SolarGrid.Web.Helpers
{
    /// <summary>
    /// Global filter that handles expired sessions.
    /// </summary>
    public class SessionExpiredFilter : IAsyncExceptionFilter
    {
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        /// <summary>
        /// Receives the TempData factory, used to show a message on the login page.
        /// </summary>
        public SessionExpiredFilter(ITempDataDictionaryFactory tempDataFactory)
        {
            _tempDataFactory = tempDataFactory;
        }

        /// <summary>
        /// Runs when a controller throws. Only handles ApiException with status 401.
        /// </summary>
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is ApiException { StatusCode: 401 })
            {
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var tempData = _tempDataFactory.GetTempData(context.HttpContext);
                tempData["Error"] = "Your session has expired. Please log in again.";

                context.Result = new RedirectToActionResult("Login", "Account", null);
                context.ExceptionHandled = true;
            }
        }
    }
}
