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
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly SlotService _slotService;

        /// <summary>
        /// Initializes a new instance of SlotsController.
        /// </summary>
        public SlotsController(SlotService slotService)
        {
            _slotService = slotService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Staff)]
        /// <summary>
        /// Executes the Create operation.
        /// </summary>
        public async Task<IActionResult> Create([FromBody] CreateSlotRequest dto)
        {
            var slot = await _slotService.CreateAsync(dto);
            return Created($"/api/stations/{slot.StationId}/slots", slot);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Staff)]
        /// <summary>
        /// Executes the Update operation.
        /// </summary>
        public async Task<IActionResult> Update(string id, [FromBody] UpdateSlotRequest dto)
        {
            var slot = await _slotService.UpdateAsync(id, dto);
            return Ok(slot);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Staff)]
        /// <summary>
        /// Executes the Delete operation.
        /// </summary>
        public async Task<IActionResult> Delete(string id)
        {
            await _slotService.DeleteAsync(id);
            return NoContent();
        }
    }
}
