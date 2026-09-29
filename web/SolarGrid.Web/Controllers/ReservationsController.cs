/*
 * File:        ReservationsController.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Web controller for viewing and managing energy reservations,
 *              including creating, editing, approving and cancelling reservations.
 * Created:     29/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Web.Helpers;
using SolarGrid.Web.Models;
using SolarGrid.Web.Services;
using System.Globalization;


namespace SolarGrid.Web.Controllers
{
    [Authorize(Roles = Roles.Staff)]
    public class ReservationsController : Controller
    {
        private readonly ApiClient _apiClient;

        public ReservationsController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Displays the reservation list with optional filters.
        /// </summary>
        public async Task<IActionResult> Index(
            string? status,
            string? stationId,
            string? nic)
        {
            var queryParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(status))
            {
                queryParts.Add(
                    $"status={Uri.EscapeDataString(status)}");
            }

            if (!string.IsNullOrWhiteSpace(stationId))
            {
                queryParts.Add(
                    $"stationId={Uri.EscapeDataString(stationId)}");
            }

            if (!string.IsNullOrWhiteSpace(nic))
            {
                queryParts.Add(
                    $"nic={Uri.EscapeDataString(nic)}");
            }

            var query = queryParts.Count > 0
                ? "?" + string.Join("&", queryParts)
                : string.Empty;

            var bookings =
                await _apiClient.GetAsync<List<BookingRecord>>(
                    $"reservations{query}");

            ViewBag.SelectedStatus = status;
            ViewBag.SelectedStationId = stationId;
            ViewBag.SelectedNic = nic;

            return View(bookings ?? new List<BookingRecord>());
        }

        /// <summary>
        /// Displays details for a single reservation.
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var booking =
                await _apiClient.GetAsync<BookingRecord>(
                    $"reservations/{Uri.EscapeDataString(id)}");

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        /// <summary>
        /// Displays the create reservation form with active prosumers
        /// and active stations.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateReservationViewModel
            {
                ReservationDate = DateTime.Today
            };

            try
            {
                // M2 API: get active prosumers.
                var prosumers =
                    await _apiClient.GetAsync<List<ProsumerOption>>(
                        "prosumers?status=Active");

                model.Prosumers =
                    prosumers ?? new List<ProsumerOption>();
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Prosumer API failed: {ex.Message}";
            }

            try
            {
                // M3 API: get active stations.
                var stations =
                    await _apiClient.GetAsync<List<StationOption>>(
                        "stations?activeOnly=true");

                model.Stations =
                    stations ?? new List<StationOption>();
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Station API failed: {ex.Message}";
            }

            return View(model);
        }

        /// <summary>
        /// Loads available slots for a selected station and date.
        /// This endpoint is called by the Create and Edit Reservation pages.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Slots(
            string stationId,
            string date)
        {
            if (string.IsNullOrWhiteSpace(stationId) ||
                string.IsNullOrWhiteSpace(date))
            {
                return BadRequest(new
                {
                    message = "Station and date are required."
                });
            }

            if (!DateTime.TryParse(
                    date,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var selectedDate))
            {
                return BadRequest(new
                {
                    message = "Invalid date."
                });
            }

            try
            {
                var formattedDate =
                    selectedDate.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture);

                // M3 API:
                // GET /api/stations/{stationId}/slots?date=yyyy-MM-dd
                var slots =
                    await _apiClient.GetAsync<List<SlotOption>>(
                        $"stations/{Uri.EscapeDataString(stationId)}/slots?date={formattedDate}");

                var availableSlots =
                    (slots ?? new List<SlotOption>())
                    .Where(s =>
                        s.IsAvailable &&
                        s.AvailableSlots > 0)
                    .Select(s => new
                    {
                        id = s.Id,
                        startTime = s.StartTime,
                        endTime = s.EndTime,
                        availableSlots = s.AvailableSlots
                    })
                    .ToList();

                return Json(availableSlots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        $"Unable to load slots: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Creates a new energy reservation through the central API.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateFormData(model);
                return View(model);
            }

            try
            {
                // Get the slots again so that the selected slot can
                // be validated before creating the reservation.
                var slots =
                    await _apiClient.GetAsync<List<SlotOption>>(
                        $"stations/{Uri.EscapeDataString(model.StationId)}/slots?date={model.ReservationDate:yyyy-MM-dd}");

                var selectedSlot =
                    slots?.FirstOrDefault(
                        s => s.Id == model.SlotId);

                if (selectedSlot == null)
                {
                    ModelState.AddModelError(
                        "SlotId",
                        "The selected time slot is no longer available.");

                    await LoadCreateFormData(model);
                    return View(model);
                }

                if (!selectedSlot.IsAvailable ||
                    selectedSlot.AvailableSlots <= 0)
                {
                    ModelState.AddModelError(
                        "SlotId",
                        "The selected time slot is no longer available.");

                    await LoadCreateFormData(model);
                    return View(model);
                }

                var request = new
                {
                    prosumerNic = model.ProsumerNic,
                    stationId = model.StationId,
                    slotId = model.SlotId,
                    reservationTime = selectedSlot.StartTime,
                    type = model.Type,
                    energyKwh = model.EnergyKwh
                };

                await _apiClient.PostAsync<object>(
                    "reservations",
                    request);

                TempData["Success"] =
                    "Reservation created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to create reservation: {ex.Message}");

                await LoadCreateFormData(model);
                return View(model);
            }
        }

        /// <summary>
        /// Displays the edit reservation form.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            try
            {
                var booking =
                    await _apiClient.GetAsync<BookingRecord>(
                        $"reservations/{Uri.EscapeDataString(id)}");

                if (booking == null)
                {
                    return NotFound();
                }

                if (booking.Status == "Cancelled" ||
                    booking.Status == "Completed")
                {
                    TempData["Error"] =
                        "This reservation can no longer be edited.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new UpdateReservationViewModel
                {
                    Id = booking.Id,
                    ProsumerNic = booking.ProsumerNic,
                    StationId = booking.StationId,
                    ReservationDate = booking.ReservationDate,
                    SlotId = booking.SlotId,
                    Type = booking.Type,
                    EnergyKwh = booking.EnergyKwh
                };

                await LoadEditFormData(model);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to load reservation: {ex.Message}";

                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Updates an existing reservation through the central API.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            UpdateReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadEditFormData(model);
                return View(model);
            }

            try
            {
                // Get the selected slot so that the reservation time
                // sent to the API matches the selected slot.
                var slots =
                    await _apiClient.GetAsync<List<SlotOption>>(
                        $"stations/{Uri.EscapeDataString(model.StationId)}/slots?date={model.ReservationDate:yyyy-MM-dd}");

                var selectedSlot =
                    slots?.FirstOrDefault(
                        s => s.Id == model.SlotId);

                if (selectedSlot == null)
                {
                    ModelState.AddModelError(
                        "SlotId",
                        "The selected time slot could not be found.");

                    await LoadEditFormData(model);
                    return View(model);
                }

                var request = new
                {
                    prosumerNic = model.ProsumerNic,
                    stationId = model.StationId,
                    slotId = model.SlotId,
                    reservationTime = selectedSlot.StartTime,
                    type = model.Type,
                    energyKwh = model.EnergyKwh
                };

                // ApiClient.PutAsync is non-generic.
                await _apiClient.PutAsync(
                    $"reservations/{Uri.EscapeDataString(model.Id)}",
                    request);

                TempData["Success"] =
                    "Reservation updated successfully.";

                return RedirectToAction(nameof(Details), new
                {
                    id = model.Id
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to update reservation: {ex.Message}");

                await LoadEditFormData(model);
                return View(model);
            }
        }

        /// <summary>
        /// Loads all required data for the edit reservation form.
        /// </summary>
        private async Task LoadEditFormData(
            UpdateReservationViewModel model)
        {
            try
            {
                var prosumers =
                    await _apiClient.GetAsync<List<ProsumerOption>>(
                        "prosumers?status=Active");

                var stations =
                    await _apiClient.GetAsync<List<StationOption>>(
                        "stations?activeOnly=true");

                model.Prosumers =
                    prosumers ?? new List<ProsumerOption>();

                model.Stations =
                    stations ?? new List<StationOption>();

                if (!string.IsNullOrWhiteSpace(model.StationId) &&
                    model.ReservationDate != default)
                {
                    var slots =
                        await _apiClient.GetAsync<List<SlotOption>>(
                            $"stations/{Uri.EscapeDataString(model.StationId)}/slots?date={model.ReservationDate:yyyy-MM-dd}");

                    model.Slots =
                        slots ?? new List<SlotOption>();
                }
                else
                {
                    model.Slots =
                        new List<SlotOption>();
                }
            }
            catch
            {
                model.Prosumers ??=
                    new List<ProsumerOption>();

                model.Stations ??=
                    new List<StationOption>();

                model.Slots ??=
                    new List<SlotOption>();
            }
        }

        /// <summary>
        /// Reloads the prosumer and station data when the create
        /// form needs to be displayed again after validation errors.
        /// </summary>
        private async Task LoadCreateFormData(
            CreateReservationViewModel model)
        {
            try
            {
                var prosumers =
                    await _apiClient.GetAsync<List<ProsumerOption>>(
                        "prosumers?status=Active");

                var stations =
                    await _apiClient.GetAsync<List<StationOption>>(
                        "stations?activeOnly=true");

                model.Prosumers =
                    prosumers ?? new List<ProsumerOption>();

                model.Stations =
                    stations ?? new List<StationOption>();
            }
            catch
            {
                model.Prosumers ??=
                    new List<ProsumerOption>();

                model.Stations ??=
                    new List<StationOption>();
            }
        }

        /// <summary>
        /// Approves a pending reservation through the central API.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            try
            {
                // PatchAsync is non-generic in the existing ApiClient.
                await _apiClient.PatchAsync(
                    $"reservations/{Uri.EscapeDataString(id)}/approve",
                    new { });

                TempData["Success"] =
                    "Reservation approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to approve reservation: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Cancels a reservation through the central API.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            try
            {
                // PatchAsync is non-generic in the existing ApiClient.
                await _apiClient.PatchAsync(
                    $"reservations/{Uri.EscapeDataString(id)}/cancel",
                    new { });

                TempData["Success"] =
                    "Reservation cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to cancel reservation: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}