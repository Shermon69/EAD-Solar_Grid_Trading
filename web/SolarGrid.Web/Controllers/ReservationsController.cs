/*
 * File:        ReservationsController.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Web controller for viewing and managing energy reservations,
 *              including creating, editing, approving and cancelling reservations.
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
            // Stores the injected API client for communicating with the central Web API.
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
            // Builds the query parameters based on the filters selected by the staff user.
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

            // Retrieves the filtered reservation records through the central API.
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
            // Validates that a reservation ID was supplied before requesting the details.
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            // Retrieves the selected reservation details from the central API.
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
            // Initializes the create reservation form with today's date as the default reservation date.
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
            // Validates that both the station ID and reservation date are provided.
            if (string.IsNullOrWhiteSpace(stationId) ||
                string.IsNullOrWhiteSpace(date))
            {
                return BadRequest(new
                {
                    message = "Station and date are required."
                });
            }

            // Converts the supplied date string into a DateTime value for slot retrieval.
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
                // Formats the selected date for the station slot API request.
                var formattedDate =
                    selectedDate.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture);

                // M3 API:
                // GET /api/stations/{stationId}/slots?date=yyyy-MM-dd
                var slots =
                    await _apiClient.GetAsync<List<SlotOption>>(
                        $"stations/{Uri.EscapeDataString(stationId)}/slots?date={formattedDate}");

                // Filters the returned slots to include only those that still have available capacity.
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
                // Returns an error response when the slot information cannot be retrieved.
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
            // Validates the submitted reservation form before communicating with the API.
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

                // Finds the slot selected by the staff user.
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

                // Verifies that the selected slot still has available capacity before creating the reservation.
                if (!selectedSlot.IsAvailable ||
                    selectedSlot.AvailableSlots <= 0)
                {
                    ModelState.AddModelError(
                        "SlotId",
                        "The selected time slot is no longer available.");

                    await LoadCreateFormData(model);
                    return View(model);
                }

                // Creates the reservation request using the selected prosumer, station, slot and booking details.
                var request = new
                {
                    prosumerNic = model.ProsumerNic,
                    stationId = model.StationId,
                    slotId = model.SlotId,
                    reservationTime = selectedSlot.StartTime,
                    type = model.Type,
                    energyKwh = model.EnergyKwh
                };

                // Sends the new reservation request to the central Web API.
                await _apiClient.PostAsync<object>(
                    "reservations",
                    request);

                TempData["Success"] =
                    "Reservation created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Displays the API error and reloads the form data when reservation creation fails.
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
            // Validates that a reservation ID was supplied before loading the edit form.
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            try
            {
                // Retrieves the selected reservation from the central API.
                var booking =
                    await _apiClient.GetAsync<BookingRecord>(
                        $"reservations/{Uri.EscapeDataString(id)}");

                if (booking == null)
                {
                    return NotFound();
                }

                // Prevents cancelled or completed reservations from being edited.
                if (booking.Status == "Cancelled" ||
                    booking.Status == "Completed")
                {
                    TempData["Error"] =
                        "This reservation can no longer be edited.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                // Populates the edit model using the existing reservation details.
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

                // Loads the active prosumers, stations and available slots required by the edit form.
                await LoadEditFormData(model);

                return View(model);
            }
            catch (Exception ex)
            {
                // Displays the error and returns the user to the reservation list if loading fails.
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
            // Validates the submitted edit form before sending the update request.
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

                // Finds the selected slot from the available slots returned by the API.
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

                // Creates the update request using the selected reservation details.
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
                // Sends the updated reservation details to the central Web API.
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
                // Displays the API error and reloads the edit form when the update fails.
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
            // Loads the active prosumers independently so that other API failures do not prevent the dropdown from loading.
            // Load active prosumers independently.
            // A station API failure should not prevent the prosumer
            // dropdown from being populated.
            try
            {
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

                model.Prosumers =
                    new List<ProsumerOption>();
            }

            // Loads the active solar stations independently.
            // Load active stations independently.
            try
            {
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

                model.Stations =
                    new List<StationOption>();
            }

            // Loads available slots when a valid station and reservation date are selected.
            // Load slots independently when a station and reservation
            // date are available.
            if (!string.IsNullOrWhiteSpace(model.StationId) &&
                model.ReservationDate != default)
            {
                try
                {
                    var slots =
                        await _apiClient.GetAsync<List<SlotOption>>(
                            $"stations/{Uri.EscapeDataString(model.StationId)}/slots?date={model.ReservationDate:yyyy-MM-dd}");

                    model.Slots =
                        slots ?? new List<SlotOption>();
                }
                catch (Exception ex)
                {
                    TempData["Error"] =
                        $"Slot API failed: {ex.Message}";

                    model.Slots =
                        new List<SlotOption>();
                }
            }
            else
            {
                model.Slots =
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
            // Reloads the active prosumers and stations so the create form can be displayed again.
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
                // Ensures the form collections are initialized even when an API request fails.
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
            // Validates that a reservation ID was supplied before sending the approval request.
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            try
            {
                // PatchAsync is non-generic in the existing ApiClient.
                // Sends the approval request to the central Web API.
                await _apiClient.PatchAsync(
                    $"reservations/{Uri.EscapeDataString(id)}/approve",
                    new { });

                TempData["Success"] =
                    "Reservation approved successfully.";
            }
            catch (Exception ex)
            {
                // Displays the API error if the reservation approval fails.
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
            // Validates that a reservation ID was supplied before sending the cancellation request.
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            try
            {
                // PatchAsync is non-generic in the existing ApiClient.
                // Sends the cancellation request to the central Web API.
                await _apiClient.PatchAsync(
                    $"reservations/{Uri.EscapeDataString(id)}/cancel",
                    new { });

                TempData["Success"] =
                    "Reservation cancelled successfully.";
            }
            catch (Exception ex)
            {
                // Displays the API error if the reservation cancellation fails.
                TempData["Error"] =
                    $"Unable to cancel reservation: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}