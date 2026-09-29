/*
 * File:        ReservationService.cs
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Business logic for energy reservation management.
 * Created:     29/09/2026
 */

using MongoDB.Driver;
using SolarGrid.Api.Data;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services
{
    /// <summary>
    /// Handles creation, updating, cancellation, approval and retrieval
    /// of energy reservations.
    /// </summary>
    public class ReservationService
    {
        private readonly MongoDbContext _db;

        /// <summary>
        /// Receives the MongoDB database context through dependency injection.
        /// </summary>
        public ReservationService(MongoDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets reservations based on the optional filters.
        /// </summary>
        public async Task<List<ReservationResponse>> GetReservationsAsync(
            string? status = null,
            string? stationId = null,
            string? nic = null)
        {
            var filter = Builders<EnergyReservation>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(status))
            {
                filter &= Builders<EnergyReservation>.Filter.Eq(
                    r => r.Status,
                    status);
            }

            if (!string.IsNullOrWhiteSpace(stationId))
            {
                filter &= Builders<EnergyReservation>.Filter.Eq(
                    r => r.StationId,
                    stationId);
            }

            if (!string.IsNullOrWhiteSpace(nic))
            {
                filter &= Builders<EnergyReservation>.Filter.Eq(
                    r => r.ProsumerNic,
                    nic);
            }

            var reservations = await _db.Reservations
                .Find(filter)
                .SortByDescending(r => r.ReservationTime)
                .ToListAsync();

            var responses = new List<ReservationResponse>();

            foreach (var reservation in reservations)
            {
                responses.Add(await MapToResponseAsync(reservation));
            }

            return responses;
        }

        /// <summary>
        /// Gets a single reservation by its ID.
        /// </summary>
        public async Task<ReservationResponse> GetReservationAsync(string id)
        {
            var reservation = await _db.Reservations
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw BusinessRuleException.NotFound(
                    "Reservation was not found.");
            }

            return await MapToResponseAsync(reservation);
        }

        /// <summary>
        /// Creates a new energy reservation after validating the
        /// reservation date, station and available slot.
        /// </summary>
        public async Task<ReservationResponse> CreateReservationAsync(
            CreateReservationRequest request,
            string prosumerNic)
        {
            var now = DateTime.UtcNow;
            var reservationTime = EnsureUtc(request.ReservationTime);

            ValidateReservationWindow(reservationTime, now);

            var station = await _db.Stations
                .Find(s => s.Id == request.StationId && s.IsActive)
                .FirstOrDefaultAsync();

            if (station == null)
            {
                throw BusinessRuleException.NotFound(
                    "The selected station was not found or is inactive.");
            }

            var slot = await _db.Slots
                .Find(s =>
                    s.Id == request.SlotId &&
                    s.StationId == request.StationId)
                .FirstOrDefaultAsync();

            if (slot == null)
            {
                throw BusinessRuleException.NotFound(
                    "The selected energy slot was not found.");
            }

            if (!slot.IsAvailable || slot.AvailableSlots <= 0)
            {
                throw new BusinessRuleException(
                    "The selected energy slot is no longer available.");
            }

            if (reservationTime < slot.StartTime ||
                reservationTime > slot.EndTime)
            {
                throw new BusinessRuleException(
                    "The reservation time must be within the selected slot.");
            }

            var reservation = new EnergyReservation
            {
                ProsumerNic = prosumerNic,
                StationId = request.StationId,
                SlotId = request.SlotId,
                ReservationTime = reservationTime,
                Type = request.Type,
                EnergyKwh = request.EnergyKwh,
                Status = ReservationStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _db.Reservations.InsertOneAsync(reservation);

            var slotUpdate = Builders<EnergySlot>.Update
                .Inc(s => s.AvailableSlots, -1)
                .Set(
                    s => s.IsAvailable,
                    slot.AvailableSlots - 1 > 0);

            await _db.Slots.UpdateOneAsync(
                s => s.Id == slot.Id,
                slotUpdate);

            return await MapToResponseAsync(reservation);
        }

        /// <summary>
        /// Updates an existing reservation while enforcing the
        /// twelve-hour modification rule.
        /// </summary>
        public async Task<ReservationResponse> UpdateReservationAsync(
            string id,
            UpdateReservationRequest request,
            string userNic,
            bool isStaff)
        {
            var reservation = await _db.Reservations
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw BusinessRuleException.NotFound(
                    "Reservation was not found.");
            }

            if (!isStaff && reservation.ProsumerNic != userNic)
            {
                throw BusinessRuleException.Forbidden(
                    "You can only update your own reservations.");
            }

            if (reservation.Status == ReservationStatus.Cancelled ||
                reservation.Status == ReservationStatus.Completed)
            {
                throw new BusinessRuleException(
                    "This reservation can no longer be updated.");
            }

            var now = DateTime.UtcNow;

            if (reservation.ReservationTime <= now.AddHours(12))
            {
                throw new BusinessRuleException(
                    "Reservations can only be updated at least 12 hours before the reservation time.");
            }

            var newReservationTime =
                EnsureUtc(request.ReservationTime);

            ValidateReservationWindow(newReservationTime, now);

            var station = await _db.Stations
                .Find(s => s.Id == request.StationId && s.IsActive)
                .FirstOrDefaultAsync();

            if (station == null)
            {
                throw BusinessRuleException.NotFound(
                    "The selected station was not found or is inactive.");
            }

            var oldSlot = await _db.Slots
                .Find(s => s.Id == reservation.SlotId)
                .FirstOrDefaultAsync();

            var newSlot = await _db.Slots
                .Find(s =>
                    s.Id == request.SlotId &&
                    s.StationId == request.StationId)
                .FirstOrDefaultAsync();

            if (newSlot == null)
            {
                throw BusinessRuleException.NotFound(
                    "The selected energy slot was not found.");
            }

            bool changingSlot =
                reservation.SlotId != request.SlotId;

            if (changingSlot &&
                (!newSlot.IsAvailable || newSlot.AvailableSlots <= 0))
            {
                throw new BusinessRuleException(
                    "The selected energy slot is no longer available.");
            }

            if (newReservationTime < newSlot.StartTime ||
                newReservationTime > newSlot.EndTime)
            {
                throw new BusinessRuleException(
                    "The reservation time must be within the selected slot.");
            }

            reservation.StationId = request.StationId;
            reservation.SlotId = request.SlotId;
            reservation.ReservationTime = newReservationTime;
            reservation.Type = request.Type;
            reservation.EnergyKwh = request.EnergyKwh;
            reservation.UpdatedAt = now;

            await _db.Reservations.ReplaceOneAsync(
                r => r.Id == reservation.Id,
                reservation);

            if (changingSlot && oldSlot != null)
            {
                await _db.Slots.UpdateOneAsync(
                    s => s.Id == oldSlot.Id,
                    Builders<EnergySlot>.Update
                        .Inc(s => s.AvailableSlots, 1)
                        .Set(s => s.IsAvailable, true));

                await _db.Slots.UpdateOneAsync(
                    s => s.Id == newSlot.Id,
                    Builders<EnergySlot>.Update
                        .Inc(s => s.AvailableSlots, -1)
                        .Set(
                            s => s.IsAvailable,
                            newSlot.AvailableSlots - 1 > 0));
            }

            return await MapToResponseAsync(reservation);
        }

        /// <summary>
        /// Cancels a reservation and returns its slot capacity.
        /// </summary>
        public async Task<ReservationResponse> CancelReservationAsync(
            string id,
            string userNic,
            bool isStaff)
        {
            var reservation = await _db.Reservations
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw BusinessRuleException.NotFound(
                    "Reservation was not found.");
            }

            if (!isStaff && reservation.ProsumerNic != userNic)
            {
                throw BusinessRuleException.Forbidden(
                    "You can only cancel your own reservations.");
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                throw new BusinessRuleException(
                    "This reservation is already cancelled.");
            }

            if (reservation.Status == ReservationStatus.Completed)
            {
                throw new BusinessRuleException(
                    "A completed reservation cannot be cancelled.");
            }

            var now = DateTime.UtcNow;

            if (reservation.ReservationTime <= now.AddHours(12))
            {
                throw new BusinessRuleException(
                    "Reservations can only be cancelled at least 12 hours before the reservation time.");
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = now;

            await _db.Reservations.ReplaceOneAsync(
                r => r.Id == reservation.Id,
                reservation);

            await _db.Slots.UpdateOneAsync(
                s => s.Id == reservation.SlotId,
                Builders<EnergySlot>.Update
                    .Inc(s => s.AvailableSlots, 1)
                    .Set(s => s.IsAvailable, true));

            return await MapToResponseAsync(reservation);
        }

        /// <summary>
        /// Approves a pending reservation and generates its QR token.
        /// </summary>
        public async Task<ReservationResponse> ApproveReservationAsync(
            string id)
        {
            var reservation = await _db.Reservations
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw BusinessRuleException.NotFound(
                    "Reservation was not found.");
            }

            if (reservation.Status != ReservationStatus.Pending)
            {
                throw new BusinessRuleException(
                    "Only pending reservations can be approved.");
            }

            reservation.Status = ReservationStatus.Approved;
            reservation.QrToken = Guid.NewGuid().ToString("N");
            reservation.UpdatedAt = DateTime.UtcNow;

            await _db.Reservations.ReplaceOneAsync(
                r => r.Id == reservation.Id,
                reservation);

            return await MapToResponseAsync(reservation);
        }

        /// <summary>
        /// Validates that a reservation is in the allowed future
        /// booking window of seven days.
        /// </summary>
        private static void ValidateReservationWindow(
            DateTime reservationTime,
            DateTime now)
        {
            if (reservationTime <= now)
            {
                throw new BusinessRuleException(
                    "The reservation time must be in the future.");
            }

            if (reservationTime > now.AddDays(7))
            {
                throw new BusinessRuleException(
                    "Reservations can only be made within the next 7 days.");
            }
        }

        /// <summary>
        /// Converts a reservation DateTime to UTC.
        /// </summary>
        private static DateTime EnsureUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;

            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();

            return DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc);
        }

        /// <summary>
        /// Converts the MongoDB reservation document into an API response
        /// and looks up the station name using the reservation StationId.
        /// </summary>
        private async Task<ReservationResponse> MapToResponseAsync(
            EnergyReservation reservation)
        {
            var station = await _db.Stations
                .Find(s => s.Id == reservation.StationId)
                .FirstOrDefaultAsync();

            return new ReservationResponse
            {
                Id = reservation.Id ?? string.Empty,
                ProsumerNic = reservation.ProsumerNic,
                StationId = reservation.StationId,
                StationName = station?.Name ?? "Unknown Station",
                SlotId = reservation.SlotId,
                ReservationTime = reservation.ReservationTime,
                Type = reservation.Type,
                EnergyKwh = reservation.EnergyKwh,
                Status = reservation.Status,
                QrToken = reservation.Status == ReservationStatus.Approved
                    ? reservation.QrToken
                    : null,
                CompletedBy = reservation.CompletedBy,
                CreatedAt = reservation.CreatedAt,
                UpdatedAt = reservation.UpdatedAt,
                CompletedAt = reservation.CompletedAt
            };
        }
    }
}