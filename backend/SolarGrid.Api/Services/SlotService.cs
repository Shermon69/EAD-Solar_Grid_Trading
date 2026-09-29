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

        public SlotService(MongoDbContext context)
        {
            _slots = context.Slots;
            _reservations = context.Reservations;
        }

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

        public async Task<EnergySlot> CreateAsync(CreateSlotRequest dto)
        {
            var slot = new EnergySlot
            {
                StationId = dto.StationId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                TotalSlots = dto.TotalSlots,
                AvailableSlots = dto.TotalSlots,
                IsAvailable = true
            };

            await _slots.InsertOneAsync(slot);
            return slot;
        }

        public async Task<EnergySlot> UpdateAsync(string id, UpdateSlotRequest dto)
        {
            var slot = await _slots.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (slot == null) throw BusinessRuleException.NotFound("Slot not found.");

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
