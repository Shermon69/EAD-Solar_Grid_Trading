/*
 * File:        HomeController.cs
 * Author:      Shermon H (IT22177964)
 * Description: The public landing page and the home pages shown after login
 *              (one for Backoffice users and one for Grid Operators).
 * Created:     28/09/2026
 */

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Web.Helpers;
using SolarGrid.Web.Models;

namespace SolarGrid.Web.Controllers
{
    /// <summary>
    /// Handles the landing page, role home pages and the error page.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// GET / - public landing page. Logged-in users are sent to their role's home page.
        /// </summary>
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.IsInRole(Roles.Backoffice))
                return RedirectToAction(nameof(Backoffice));

            if (User.IsInRole(Roles.GridOperator))
                return RedirectToAction(nameof(Operator));

            return View();
        }

        /// <summary>
        /// GET /Home/Backoffice - home page for Backoffice users with links to admin functions.
        /// </summary>
        [Authorize(Roles = Roles.Backoffice)]
        public IActionResult Backoffice()
        {
            return View();
        }

        /// <summary>
        /// GET /Home/Operator - home page for Grid Operators with links to operational tools.
        /// </summary>
        [Authorize(Roles = Roles.GridOperator)]
        public IActionResult Operator()
        {
            return View();
        }

        /// <summary>
        /// GET /Home/Error - general error page (from the MVC template).
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
