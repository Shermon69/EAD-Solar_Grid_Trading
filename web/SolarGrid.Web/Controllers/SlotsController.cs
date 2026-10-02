/*
 * File:        SlotsController.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Controller for the Slot Availability web pages.
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
    public class SlotsController : Controller
    {
        private readonly ApiClient _api;

        /// <summary>
        /// Initializes a new instance of SlotsController.
        /// </summary>
        public SlotsController(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Executes the Index operation.
        /// </summary>
        public async Task<IActionResult> Index(string stationId)
        {
            // No station chosen yet (e.g. opened from the navbar): show a station picker (added by Shermon H)
            if (string.IsNullOrEmpty(stationId))
            {
                return await SelectStation();
            }

            try
            {
                var station = await _api.GetAsync<StationViewModel>($"stations/{stationId}");
                ViewBag.StationName = station.Name;
                ViewBag.StationId = stationId;

                var slots = await _api.GetAsync<List<SlotViewModel>>($"stations/{stationId}/slots");
                return View(slots);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Stations");
            }
        }

        [HttpPost]
        /// <summary>
        /// Executes the Create operation.
        /// </summary>
        public async Task<IActionResult> Create(CreateSlotViewModel model)
        {
            try
            {
                await _api.PostAsync("slots", model);
                TempData["Success"] = "Slot created successfully.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index), new { stationId = model.StationId });
        }

        /// <summary>
        /// POST /Slots/Update - changes how many battery slots a time block offers and
        /// whether it can be booked. The API checks the booked count and station limit.
        /// (Added by Shermon H)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Executes the Update operation.
        /// </summary>
        public async Task<IActionResult> Update(string id, string stationId, int totalSlots, bool isAvailable)
        {
            try
            {
                await _api.PutAsync($"slots/{Uri.EscapeDataString(id)}", new { totalSlots, isAvailable });
                TempData["Success"] = "Slot availability updated.";
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index), new { stationId });
        }

        /// <summary>
        /// Shows the list of stations so the user can pick one to manage its slots.
        /// (Added by Shermon H)
        /// </summary>
        private async Task<IActionResult> SelectStation()
        {
            try
            {
                var stations = await _api.GetAsync<List<StationViewModel>>("stations");
                return View("SelectStation", stations);
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                TempData["Error"] = ex.Message;
                return View("SelectStation", new List<StationViewModel>());
            }
        }

        [HttpPost]
        /// <summary>
        /// Executes the Delete operation.
        /// </summary>
        public async Task<IActionResult> Delete(string id, string stationId)
        {
            try
            {
                await _api.DeleteAsync($"slots/{id}");
                TempData["Success"] = "Slot deleted successfully.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index), new { stationId });
        }
    }
}
