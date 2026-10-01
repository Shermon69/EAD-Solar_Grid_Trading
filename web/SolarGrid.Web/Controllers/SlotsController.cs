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

        public SlotsController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string stationId)
        {
            if (string.IsNullOrEmpty(stationId))
            {
                TempData["Error"] = "Please select a station first.";
                return RedirectToAction("Index", "Stations");
            }

            try
            {
                var station = await _api.GetAsync<StationViewModel>($"api/stations/{stationId}");
                ViewBag.StationName = station.Name;
                ViewBag.StationId = stationId;

                var slots = await _api.GetAsync<List<SlotViewModel>>($"api/stations/{stationId}/slots");
                return View(slots);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Stations");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSlotViewModel model)
        {
            try
            {
                await _api.PostAsync("api/slots", model);
                TempData["Success"] = "Slot created successfully.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index), new { stationId = model.StationId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id, string stationId)
        {
            try
            {
                await _api.DeleteAsync($"api/slots/{id}");
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
