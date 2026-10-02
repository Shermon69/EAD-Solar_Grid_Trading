/*
 * File:        SlotService.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Business logic for managing energy booking slots,
 *              such as adding or modifying available time blocks.
 * Created:     29/09/2026
 */

using MongoDB.Driver;
using SolarGrid.Api.Data;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services
{
    public class SlotService
    {
        private readonly IMongoCollection<EnergySlot> _slots;
        private readonly IMongoCollection<EnergyReservation> _reservations;
        private readonly IMongoCollection<SolarStation> _stations;

        /// <summary>
        /// Initializes a new instance of SlotService.
        /// </summary>
        public SlotService(MongoDbContext context)
        {
            _slots = context.Slots;
            _reservations = context.Reservations;
            _stations = context.Stations;
        }

        /// <summary>
        /// Executes the GetSlotsForStationAsync operation.
        /// </summary>
        public async Task<List<EnergySlot>> GetSlotsForStationAsync(string stationId, DateTime? date = null)
        {
            var filterBuilder = Builders<EnergySlot>.Filter;
            var filter = filterBuilder.Eq(s => s.StationId, stationId);

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1);
                filter &= filterBuilder.Gte(s => s.StartTime, startOfDay) & filterBuilder.Lt(s => s.StartTime, endOfDay);
            }

            return await _slots.Find(filter).SortBy(s => s.StartTime).ToListAsync();
        }

        /// <summary>
        /// Executes the CreateAsync operation.
        /// </summary>
        public async Task<EnergySlot> CreateAsync(CreateSlotRequest dto)
        {
            // Check station, times, slot count and overlaps (added by Shermon H)
            var (start, end) = await MicrogridValidator.ValidateNewSlotAsync(dto, _stations, _slots);

            var slot = new EnergySlot
            {
                StationId = dto.StationId,
                StartTime = start,
                EndTime = end,
                TotalSlots = dto.TotalSlots,
                AvailableSlots = dto.TotalSlots,
                IsAvailable = true
            };

            await _slots.InsertOneAsync(slot);
            return slot;
        }

        /// <summary>
        /// Executes the UpdateAsync operation.
        /// </summary>
        public async Task<EnergySlot> UpdateAsync(string id, UpdateSlotRequest dto)
        {
            var slot = await _slots.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (slot == null) throw BusinessRuleException.NotFound("Slot not found.");

            // New slot count must fit the station's battery slots (added by Shermon H)
            await MicrogridValidator.ValidateSlotUpdateAsync(slot, dto, _stations);

            var usedSlots = slot.TotalSlots - slot.AvailableSlots;
            if (dto.TotalSlots < usedSlots)
            {
                throw BusinessRuleException.Conflict($"Cannot reduce total slots below the number of already booked slots ({usedSlots}).");
            }

            slot.TotalSlots = dto.TotalSlots;
            slot.AvailableSlots = dto.TotalSlots - usedSlots;
            slot.IsAvailable = dto.IsAvailable;

            await _slots.ReplaceOneAsync(s => s.Id == id, slot);
            return slot;
        }

        /// <summary>
        /// Executes the DeleteAsync operation.
        /// </summary>
        public async Task DeleteAsync(string id)
        {
            var slot = await _slots.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (slot == null) throw BusinessRuleException.NotFound("Slot not found.");

            var reservationsCount = await _reservations.CountDocumentsAsync(r => r.SlotId == id);
            if (reservationsCount > 0)
            {
                throw BusinessRuleException.Conflict("Cannot delete a slot that has reservations.");
            }

            await _slots.DeleteOneAsync(s => s.Id == id);
        }
    }
}
