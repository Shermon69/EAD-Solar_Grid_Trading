/*
 * File:        SlotsController.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: API endpoints for managing energy booking slots.
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
    /// Energy booking slot (time block) endpoints used by Backoffice and grid operators.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly SlotService _slotService;

        /// <summary>
        /// Receives the slot service through dependency injection.
        /// </summary>
        public SlotsController(SlotService slotService)
        {
            _slotService = slotService;
        }

        /// <summary>
        /// POST /api/slots - adds a new time block of battery slots to a station (staff only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Staff)]
        public async Task<IActionResult> Create([FromBody] CreateSlotRequest dto)
        {
            var slot = await _slotService.CreateAsync(dto);
            return Created($"/api/stations/{slot.StationId}/slots", slot);
        }

        /// <summary>
        /// PUT /api/slots/{id} - changes the number of slots or switches the block on/off (staff only).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Staff)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateSlotRequest dto)
        {
            var slot = await _slotService.UpdateAsync(id, dto);
            return Ok(slot);
        }

        /// <summary>
        /// DELETE /api/slots/{id} - removes a time block that has no reservations (staff only).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Staff)]
        public async Task<IActionResult> Delete(string id)
        {
            await _slotService.DeleteAsync(id);
            return NoContent();
        }
    }
}
