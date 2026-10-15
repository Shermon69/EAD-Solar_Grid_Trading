/*
 * File:        StationsController.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: API endpoints for managing Solar Stations (nodes) and querying nearby stations.
 * Created:     29/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Models;
using SolarGrid.Api.Services;

namespace SolarGrid.Api.Controllers
{
    /// <summary>
    /// Solar station (microgrid node) endpoints used by the web app and the mobile map.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly StationService _stationService;
        private readonly SlotService _slotService;

        /// <summary>
        /// Receives the station and slot services through dependency injection.
        /// </summary>
        public StationsController(StationService stationService, SlotService slotService)
        {
            _stationService = stationService;
            _slotService = slotService;
        }

        /// <summary>
        /// GET /api/stations - lists stations. With activeOnly=true only active ones are returned (used by the mobile map and booking screens).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            var stations = await _stationService.GetAllAsync(activeOnly);
            return Ok(stations);
        }

        /// <summary>
        /// GET /api/stations/{id} - returns one station with its weekly opening hours.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var station = await _stationService.GetByIdAsync(id);
            return Ok(station);
        }

        /// <summary>
        /// GET /api/stations/nearby?lat=..&lng=..&radiusKm=.. - returns active stations inside the radius, nearest first.
        /// </summary>
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radiusKm)
        {
            var stations = await _stationService.GetNearbyAsync(lat, lng, radiusKm);
            return Ok(stations);
        }

        /// <summary>
        /// POST /api/stations - creates a new solar station (Backoffice only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Backoffice)]
        public async Task<IActionResult> Create([FromBody] CreateStationRequest dto)
        {
            var station = await _stationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = station.Id }, station);
        }

        /// <summary>
        /// PUT /api/stations/{id} - updates a station's details and schedule (Backoffice only).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Backoffice)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateStationRequest dto)
        {
            var station = await _stationService.UpdateAsync(id, dto);
            return Ok(station);
        }

        /// <summary>
        /// PATCH /api/stations/{id}/deactivate - deactivates a station. Blocked if it has active future reservations (rule R8).
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = Roles.Backoffice)]
        public async Task<IActionResult> Deactivate(string id)
        {
            await _stationService.DeactivateAsync(id);
            return NoContent();
        }

        /// <summary>
        /// PATCH /api/stations/{id}/activate - makes a deactivated station active again (Backoffice only).
        /// </summary>
        [HttpPatch("{id}/activate")]
        [Authorize(Roles = Roles.Backoffice)]
        public async Task<IActionResult> Activate(string id)
        {
            await _stationService.ActivateAsync(id);
            return NoContent();
        }

        // --- Slots related to a specific station ---

        /// <summary>
        /// GET /api/stations/{id}/slots?date=yyyy-MM-dd - lists the station's time blocks, optionally for one day.
        /// </summary>
        [HttpGet("{id}/slots")]
        public async Task<IActionResult> GetSlots(string id, [FromQuery] DateTime? date)
        {
            var slots = await _slotService.GetSlotsForStationAsync(id, date);
            return Ok(slots);
        }
    }
}
