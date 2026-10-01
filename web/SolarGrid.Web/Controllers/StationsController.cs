/*
 * File:        StationsController.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Controller for the Node Management web pages.
 * Created:     29/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Web.Helpers;
using SolarGrid.Web.Models;
using SolarGrid.Web.Services;

namespace SolarGrid.Web.Controllers
{
    [Authorize(Roles = "Backoffice,GridOperator")]
    public class StationsController : Controller
    {
        private readonly ApiClient _api;

        public StationsController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var stations = await _api.GetAsync<List<StationViewModel>>("api/stations");
                return View(stations);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return View(new List<StationViewModel>());
            }
        }

        [Authorize(Roles = "Backoffice")]
        public IActionResult Create()
        {
            return View(new CreateStationViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> Create(CreateStationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _api.PostAsync("api/stations", model);
                TempData["Success"] = "Station created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> ToggleStatus(string id, bool currentStatus)
        {
            try
            {
                var action = currentStatus ? "deactivate" : "activate";
                await _api.PatchAsync($"api/stations/{id}/{action}");
                TempData["Success"] = $"Station {action}d successfully.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
