/*
 * File:        MicrogridValidator.cs
 * Author:      Shermon H (IT22177964)
 * Description: Validation rules for solar stations and energy booking slots.
 *              Used by StationService and SlotService before saving, so that
 *              invalid data (wrong GPS, overlapping slots, more slots than the
 *              station's batteries, etc.) is rejected by the API.
 */

using MongoDB.Driver;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services
{
    /// <summary>
    /// Business rules for station and slot data.
    /// </summary>
    public static class MicrogridValidator
    {
        private static readonly string[] WeekDays =
            { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        // A single slot block can be at most this long
        private const int MaxSlotHours = 12;

        // Slots can be created up to this many days ahead
        private const int MaxDaysAhead = 30;

        /// <summary>
        /// Checks a station's details: GPS location, capacity, battery slots and schedule.
        /// Throws BusinessRuleException (400) with a clear message if something is wrong.
        /// </summary>
        public static void ValidateStation(CreateStationRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new BusinessRuleException("Station name is required.");

            if (dto.Latitude < -90 || dto.Latitude > 90)
                throw new BusinessRuleException("Latitude must be between -90 and 90.");

            if (dto.Longitude < -180 || dto.Longitude > 180)
                throw new BusinessRuleException("Longitude must be between -180 and 180.");

            if (dto.CapacityKw <= 0)
                throw new BusinessRuleException("Capacity must be greater than 0 kW.");

            if (dto.BatterySlots < 1)
                throw new BusinessRuleException("A station must have at least 1 battery slot.");

            ValidateSchedule(dto.Schedule);
        }

        /// <summary>
        /// Checks the weekly schedule: real day names, no day listed twice,
        /// times in HH:mm format and opening time before closing time.
        /// </summary>
        private static void ValidateSchedule(List<OperatingHours> schedule)
        {
            if (schedule == null)
                return;

            var seenDays = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var day in schedule)
            {
                if (!WeekDays.Contains(day.Day, StringComparer.OrdinalIgnoreCase))
                    throw new BusinessRuleException($"'{day.Day}' is not a valid day of the week.");

                if (!seenDays.Add(day.Day))
                    throw new BusinessRuleException($"{day.Day} appears more than once in the schedule.");

                if (!TimeSpan.TryParse(day.OpenTime, out var open) || !TimeSpan.TryParse(day.CloseTime, out var close))
                    throw new BusinessRuleException($"Times for {day.Day} must be in HH:mm format.");

                if (open >= close)
                    throw new BusinessRuleException($"On {day.Day}, the opening time must be before the closing time.");
            }
        }

        /// <summary>
        /// Checks a new slot block before it is saved and returns its start and end in UTC.
        /// Rules: the station exists and is active, the end is after the start, the slot is in
        /// the future (up to 30 days ahead, at most 12 hours long), the number of slots fits the
        /// station's battery slots, and it does not overlap another slot at the same station.
        /// </summary>
        public static async Task<(DateTime Start, DateTime End)> ValidateNewSlotAsync(
            CreateSlotRequest dto,
            IMongoCollection<SolarStation> stations,
            IMongoCollection<EnergySlot> slots)
        {
            var station = await stations.Find(s => s.Id == dto.StationId).FirstOrDefaultAsync();
            if (station == null)
                throw BusinessRuleException.NotFound("Station not found.");

            if (!station.IsActive)
                throw new BusinessRuleException("Slots cannot be added to a deactivated station.");

            var start = ToUtc(dto.StartTime);
            var end = ToUtc(dto.EndTime);

            if (end <= start)
                throw new BusinessRuleException("End time must be after the start time.");

            if (start <= DateTime.UtcNow)
                throw new BusinessRuleException("Slots can only be created for a future time.");

            if (start > DateTime.UtcNow.AddDays(MaxDaysAhead))
                throw new BusinessRuleException($"Slots can be created at most {MaxDaysAhead} days ahead.");

            if ((end - start).TotalHours > MaxSlotHours)
                throw new BusinessRuleException($"A slot block can be at most {MaxSlotHours} hours long.");

            ValidateSlotCount(dto.TotalSlots, station);

            // Two blocks overlap if one starts before the other ends and ends after the other starts
            var overlaps = await slots.Find(s =>
                    s.StationId == dto.StationId &&
                    s.StartTime < end &&
                    s.EndTime > start)
                .AnyAsync();

            if (overlaps)
                throw BusinessRuleException.Conflict("This time overlaps with another slot block at the same station.");

            return (start, end);
        }

        /// <summary>
        /// Checks a change to an existing slot: the new number of slots must still fit the
        /// station's battery slots. (SlotService also checks it is not below the booked count.)
        /// </summary>
        public static async Task ValidateSlotUpdateAsync(
            EnergySlot slot,
            UpdateSlotRequest dto,
            IMongoCollection<SolarStation> stations)
        {
            var station = await stations.Find(s => s.Id == slot.StationId).FirstOrDefaultAsync();
            if (station == null)
                throw BusinessRuleException.NotFound("The station for this slot was not found.");

            ValidateSlotCount(dto.TotalSlots, station);
        }

        /// <summary>
        /// The number of slots in a block must be at least 1 and not more than the station's battery slots.
        /// </summary>
        private static void ValidateSlotCount(int totalSlots, SolarStation station)
        {
            if (totalSlots < 1)
                throw new BusinessRuleException("A slot block must offer at least 1 battery slot.");

            if (totalSlots > station.BatterySlots)
                throw new BusinessRuleException(
                    $"{station.Name} only has {station.BatterySlots} battery slots, so a block cannot offer {totalSlots}.");
        }

        /// <summary>
        /// Converts a time to UTC. Times typed in the web form have no time zone, so they are
        /// treated as the server's local time (Sri Lanka) and converted.
        /// </summary>
        private static DateTime ToUtc(DateTime time) =>
            time.Kind == DateTimeKind.Utc ? time : time.ToUniversalTime();
    }
}
