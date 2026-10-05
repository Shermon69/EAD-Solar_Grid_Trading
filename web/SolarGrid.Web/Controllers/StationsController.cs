/*
 * File:        StationsController.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Controller for the Node Management web pages.
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

        /// <summary>
        /// Initializes a new instance of StationsController.
        /// </summary>
        public StationsController(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Executes the Index operation.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var stations = await _api.GetAsync<List<StationViewModel>>("stations");
                return View(stations);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return View(new List<StationViewModel>());
            }
        }

        [Authorize(Roles = "Backoffice")]
        /// <summary>
        /// Executes the Create operation.
        /// </summary>
        public IActionResult Create()
        {
            return View(new CreateStationViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Backoffice")]
        /// <summary>
        /// Executes the Create operation.
        /// </summary>
        public async Task<IActionResult> Create(CreateStationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _api.PostAsync("stations", model);
                TempData["Success"] = "Station created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        /// <summary>
        /// GET /Stations/Edit/{id} - loads a station from the API into the edit form,
        /// including a row for every day of the week. (Added by Shermon H)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Backoffice")]
        /// <summary>
        /// Executes the Edit operation.
        /// </summary>
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var station = await _api.GetAsync<StationViewModel>($"stations/{Uri.EscapeDataString(id)}");
                return View(new EditStationViewModel
                {
                    Id = station.Id ?? id,
                    Name = station.Name,
                    Address = station.Address,
                    Latitude = station.Latitude,
                    Longitude = station.Longitude,
                    CapacityKw = station.CapacityKw,
                    BatterySlots = station.BatterySlots,
                    IsActive = station.IsActive,
                    Schedule = EditStationViewModel.BuildWeek(station.Schedule)
                });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST /Stations/Edit/{id} - sends the updated details and schedule to the API.
        /// Only the days marked as open are saved in the schedule. (Added by Shermon H)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Backoffice")]
        /// <summary>
        /// Executes the Edit operation.
        /// </summary>
        public async Task<IActionResult> Edit(string id, EditStationViewModel model)
        {
            model.Id = id;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _api.PutAsync($"stations/{Uri.EscapeDataString(id)}", new
                {
                    name = model.Name,
                    address = model.Address,
                    latitude = model.Latitude,
                    longitude = model.Longitude,
                    capacityKw = model.CapacityKw,
                    batterySlots = model.BatterySlots,
                    isActive = model.IsActive,
                    schedule = model.Schedule
                        .Where(d => d.IsOpen)
                        .Select(d => new { day = d.Day, openTime = d.OpenTime, closeTime = d.CloseTime })
                });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            TempData["Success"] = $"{model.Name} was updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Backoffice")]
        /// <summary>
        /// Executes the ToggleStatus operation.
        /// </summary>
        public async Task<IActionResult> ToggleStatus(string id, bool currentStatus)
        {
            try
            {
                var action = currentStatus ? "deactivate" : "activate";
                await _api.PatchAsync($"stations/{id}/{action}");
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
