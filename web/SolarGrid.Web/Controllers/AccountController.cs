/*
 * File:        AccountController.cs
 * Author:      Shermon H (IT22177964)
 * Description: Login and logout for the web app. The login details are checked
 *              by the Web API. If they are correct, the web app saves the user's
 *              role and API token in a secure cookie and sends the user to the
 *              home page for their role.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Web.Helpers;
using SolarGrid.Web.Models;
using SolarGrid.Web.Services;

namespace SolarGrid.Web.Controllers
{
    /// <summary>
    /// Handles /Account/Login, /Account/Logout and /Account/AccessDenied.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ApiClient _api;

        /// <summary>
        /// Receives the ApiClient through dependency injection.
        /// </summary>
        public AccountController(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// GET /Account/Login - shows the login form. Logged-in users go straight to their home page.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToRoleHome(User.FindFirstValue(ClaimTypes.Role));

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        /// <summary>
        /// POST /Account/Login - sends the NIC and password to the API. On success, signs the
        /// user in with a cookie and redirects by role. Prosumers are sent to the mobile app.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            LoginResponse result;
            try
            {
                result = await _api.PostAsync<LoginResponse>("auth/login",
                    new { nic = model.Nic, password = model.Password });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            // The web app is only for staff. Prosumers use the mobile app
            if (result.Role != Roles.Backoffice && result.Role != Roles.GridOperator)
            {
                ModelState.AddModelError(string.Empty, "The web portal is for Backoffice and Grid Operator staff. Prosumers please use the mobile app.");
                return View(model);
            }

            // Save who the user is, their role and the API token inside the login cookie
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Nic),
                new(ClaimTypes.Name, result.FullName),
                new(ClaimTypes.Role, result.Role),
                new(AppClaims.AccessToken, result.Token)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = false });

            TempData["Success"] = $"Welcome back, {result.FullName}!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToRoleHome(result.Role);
        }

        /// <summary>
        /// POST /Account/Logout - removes the login cookie and goes back to the landing page.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// GET /Account/AccessDenied - shown when a user opens a page their role cannot use.
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Sends Backoffice users to the Backoffice home and Grid Operators to the Operator home.
        /// </summary>
        private IActionResult RedirectToRoleHome(string? role)
        {
            return role == Roles.Backoffice
                ? RedirectToAction("Backoffice", "Home")
                : RedirectToAction("Operator", "Home");
        }
    }
}