/*
 * File:        UsersController.cs
 * Author:      Shermon H (IT22177964)
 * Description: Staff User Management pages (Backoffice only). Lists, creates,
 *              edits, activates and deactivates Backoffice and Grid Operator
 *              accounts. Every action calls the Web API; the rules are checked there.
 * Created:     29/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Web.Helpers;
using SolarGrid.Web.Models;
using SolarGrid.Web.Services;

namespace SolarGrid.Web.Controllers
{
    /// <summary>
    /// Handles /Users pages.
    /// </summary>
    [Authorize(Roles = Roles.Backoffice)]
    public class UsersController : Controller
    {
        private readonly ApiClient _api;

        /// <summary>
        /// Receives the ApiClient through dependency injection.
        /// </summary>
        public UsersController(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// GET /Users - list of staff users. Can be filtered by role and searched by name, NIC or email.
        /// </summary>
        public async Task<IActionResult> Index(string? role, string? search)
        {
            var url = string.IsNullOrEmpty(role) ? "users" : $"users?role={Uri.EscapeDataString(role)}";

            List<StaffUserViewModel> users;
            try
            {
                users = await _api.GetAsync<List<StaffUserViewModel>>(url);
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                TempData["Error"] = ex.Message;
                users = new List<StaffUserViewModel>();
            }

            // Search box: only filters what is shown on the page
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                users = users.Where(u =>
                        u.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        u.Nic.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.Role = role;
            ViewBag.Search = search;
            return View(users);
        }

        /// <summary>
        /// GET /Users/Create - empty form for a new staff user.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new StaffUserFormViewModel());
        }

        /// <summary>
        /// POST /Users/Create - sends the new user to the API.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffUserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(model.Password), "Password is required.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _api.PostAsync("users", new
                {
                    nic = model.Nic,
                    fullName = model.FullName,
                    email = model.Email,
                    phone = model.Phone,
                    address = model.Address ?? string.Empty,
                    role = model.Role,
                    password = model.Password
                });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            TempData["Success"] = $"{model.FullName} was added as a {(model.Role == Roles.GridOperator ? "Grid Operator" : "Backoffice user")}.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET /Users/Edit/{nic} - loads the user from the API into the edit form.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _api.GetAsync<StaffUserViewModel>($"users/{Uri.EscapeDataString(id)}");
                return View(new StaffUserFormViewModel
                {
                    IsEdit = true,
                    Nic = user.Nic,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role
                });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST /Users/Edit/{nic} - sends the changes to the API. The password is only
        /// changed if a new one is typed.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, StaffUserFormViewModel model)
        {
            model.IsEdit = true;
            model.Nic = id;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _api.PutAsync($"users/{Uri.EscapeDataString(id)}", new
                {
                    fullName = model.FullName,
                    email = model.Email,
                    phone = model.Phone,
                    address = model.Address ?? string.Empty,
                    role = model.Role,
                    newPassword = string.IsNullOrWhiteSpace(model.Password) ? null : model.Password
                });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            TempData["Success"] = $"{model.FullName}'s details were updated.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST /Users/Activate/{nic} - activates a staff account.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            return await ChangeStatusAsync(id, "activate", "activated");
        }

        /// <summary>
        /// POST /Users/Deactivate/{nic} - deactivates a staff account.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            return await ChangeStatusAsync(id, "deactivate", "deactivated");
        }

        /// <summary>
        /// Calls PATCH /api/users/{nic}/{action} and shows the result on the list page.
        /// </summary>
        private async Task<IActionResult> ChangeStatusAsync(string nic, string action, string pastTense)
        {
            try
            {
                await _api.PatchAsync($"users/{Uri.EscapeDataString(nic)}/{action}");
                TempData["Success"] = $"User {nic} was {pastTense}.";
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
