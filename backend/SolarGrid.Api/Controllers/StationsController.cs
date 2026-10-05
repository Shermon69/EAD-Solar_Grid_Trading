/*
 * File:        StationsController.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: API endpoints for managing Solar Stations (nodes) and querying nearby stations.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Models;
using SolarGrid.Api.Services;

namespace SolarGrid.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly StationService _stationService;
        private readonly SlotService _slotService;

        /// <summary>
        /// Initializes a new instance of StationsController.
        /// </summary>
        public StationsController(StationService stationService, SlotService slotService)
        {
            _stationService = stationService;
            _slotService = slotService;
        }

        [HttpGet]
        /// <summary>
        /// Executes the GetAll operation.
        /// </summary>
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            var stations = await _stationService.GetAllAsync(activeOnly);
            return Ok(stations);
        }

        [HttpGet("{id}")]
        /// <summary>
        /// Executes the GetById operation.
        /// </summary>
        public async Task<IActionResult> GetById(string id)
        {
            var station = await _stationService.GetByIdAsync(id);
            return Ok(station);
        }

        [HttpGet("nearby")]
        /// <summary>
        /// Executes the GetNearby operation.
        /// </summary>
        public async Task<IActionResult> GetNearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radiusKm)
        {
            var stations = await _stationService.GetNearbyAsync(lat, lng, radiusKm);
            return Ok(stations);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Backoffice)]
        /// <summary>
        /// Executes the Create operation.
        /// </summary>
        public async Task<IActionResult> Create([FromBody] CreateStationRequest dto)
        {
            var station = await _stationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = station.Id }, station);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Backoffice)]
        /// <summary>
        /// Executes the Update operation.
        /// </summary>
        public async Task<IActionResult> Update(string id, [FromBody] UpdateStationRequest dto)
        {
            var station = await _stationService.UpdateAsync(id, dto);
            return Ok(station);
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = Roles.Backoffice)]
        /// <summary>
        /// Executes the Deactivate operation.
        /// </summary>
        public async Task<IActionResult> Deactivate(string id)
        {
            await _stationService.DeactivateAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/activate")]
        [Authorize(Roles = Roles.Backoffice)]
        /// <summary>
        /// Executes the Activate operation.
        /// </summary>
        public async Task<IActionResult> Activate(string id)
        {
            await _stationService.ActivateAsync(id);
            return NoContent();
        }

        // --- Slots related to a specific station ---

        [HttpGet("{id}/slots")]
        /// <summary>
        /// Executes the GetSlots operation.
        /// </summary>
        public async Task<IActionResult> GetSlots(string id, [FromQuery] DateTime? date)
        {
            var slots = await _slotService.GetSlotsForStationAsync(id, date);
            return Ok(slots);
        }
    }
}
