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
            // Stores the injected MongoDB context for database operations.
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
            // Creates an empty filter that can be extended using the supplied search criteria.
            var filter = Builders<EnergyReservation>.Filter.Empty;

            // Adds a status filter when a reservation status is provided.
            if (!string.IsNullOrWhiteSpace(status))
            {
                filter &= Builders<EnergyReservation>.Filter.Eq(
                    r => r.Status,
                    status);
            }

            // Adds a station filter when a station ID is provided.
            if (!string.IsNullOrWhiteSpace(stationId))
            {
                filter &= Builders<EnergyReservation>.Filter.Eq(
                    r => r.StationId,
                    stationId);
            }

            // Adds a prosumer NIC filter when a NIC is provided.
            if (!string.IsNullOrWhiteSpace(nic))
            {
                filter &= Builders<EnergyReservation>.Filter.Eq(
                    r => r.ProsumerNic,
                    nic);
            }

            // Retrieves matching reservations from MongoDB in descending reservation-time order.
            var reservations = await _db.Reservations
                .Find(filter)
                .SortByDescending(r => r.ReservationTime)
                .ToListAsync();

            // Creates a collection for the reservation responses returned by the API.
            var responses = new List<ReservationResponse>();

            // Converts each reservation document into an API response.
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
            // Retrieves the reservation from MongoDB using its unique ID.
            var reservation = await _db.Reservations
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            // Ensures that the requested reservation exists.
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
            // Gets the current UTC time and converts the requested reservation time to UTC.
            var now = DateTime.UtcNow;
            var reservationTime = EnsureUtc(request.ReservationTime);

            // Validates that the requested reservation falls within the allowed seven-day window.
            ValidateReservationWindow(reservationTime, now);

            // Retrieves the selected active solar station from MongoDB.
            var station = await _db.Stations
                .Find(s => s.Id == request.StationId && s.IsActive)
                .FirstOrDefaultAsync();

            // Ensures that the selected station exists and is active.
            if (station == null)
            {
                throw BusinessRuleException.NotFound(
                    "The selected station was not found or is inactive.");
            }

            // Retrieves the selected booking slot belonging to the selected station.
            var slot = await _db.Slots
                .Find(s =>
                    s.Id == request.SlotId &&
                    s.StationId == request.StationId)
                .FirstOrDefaultAsync();

            // Ensures that the selected energy slot exists.
            if (slot == null)
            {
                throw BusinessRuleException.NotFound(
                    "The selected energy slot was not found.");
            }

            // Checks whether the selected slot still has available capacity.
            if (!slot.IsAvailable || slot.AvailableSlots <= 0)
            {
                throw new BusinessRuleException(
                    "The selected energy slot is no longer available.");
            }

            // Ensures that the requested reservation time falls within the selected slot.
            if (reservationTime < slot.StartTime ||
                reservationTime > slot.EndTime)
            {
                throw new BusinessRuleException(
                    "The reservation time must be within the selected slot.");
            }

            // Creates a new reservation using the validated booking details.
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

            // Saves the new reservation in the MongoDB collection.
            await _db.Reservations.InsertOneAsync(reservation);

            // Decreases the number of available spaces in the selected slot.
            var slotUpdate = Builders<EnergySlot>.Update
                .Inc(s => s.AvailableSlots, -1)
                .Set(
                    s => s.IsAvailable,
                    slot.AvailableSlots - 1 > 0);

            // Updates the selected slot with its new availability.
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
            // Retrieves the reservation that is being updated.
            var reservation = await _db.Reservations
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            // Ensures that the reservation exists before attempting the update.
            if (reservation == null)
            {
                throw BusinessRuleException.NotFound(
                    "Reservation was not found.");
            }

            // Prevents prosumers from updating reservations belonging to other prosumers.
            if (!isStaff && reservation.ProsumerNic != userNic)
            {
                throw BusinessRuleException.Forbidden(
                    "You can only update your own reservations.");
            }

            // Prevents cancelled or completed reservations from being modified.
            if (reservation.Status == ReservationStatus.Cancelled ||
                reservation.Status == ReservationStatus.Completed)
            {
                throw new BusinessRuleException(
                    "This reservation can no longer be updated.");
            }

            // Gets the current UTC time for the twelve-hour validation.
            var now = DateTime.UtcNow;

            // Enforces the rule that reservations must be updated at least twelve hours before the booking.
            if (reservation.ReservationTime <= now.AddHours(12))
            {
                throw new BusinessRuleException(
                    "Reservations can only be updated at least 12 hours before the reservation time.");
            }

            // Converts the requested new reservation time to UTC.
            var newReservationTime =
                EnsureUtc(request.ReservationTime);

            // Validates the new reservation time against the seven-day booking window.
            ValidateReservationWindow(newReservationTime, now);

            // Retrieves the selected active station for the updated reservation.
            var station = await _db.Stations
                .Find(s => s.Id == request.StationId && s.IsActive)
                .FirstOrDefaultAsync();

            // Ensures that the selected station exists and is active.
            if (station == null)
            {
                throw BusinessRuleException.NotFound(
                    "The selected station was not found or is inactive.");
            }

            // Retrieves the original booking slot.
            var oldSlot = await _db.Slots
                .Find(s => s.Id == reservation.SlotId)
                .FirstOrDefaultAsync();

            // Retrieves the new booking slot selected for the updated reservation.
            var newSlot = await _db.Slots
                .Find(s =>
                    s.Id == request.SlotId &&
                    s.StationId == request.StationId)
                .FirstOrDefaultAsync();

            // Ensures that the selected new slot exists.
            if (newSlot == null)
            {
                throw BusinessRuleException.NotFound(
                    "The selected energy slot was not found.");
            }

            // Determines whether the reservation is being moved to a different slot.
            bool changingSlot =
                reservation.SlotId != request.SlotId;

            // Checks availability when the reservation is moved to a different slot.
            if (changingSlot &&
                (!newSlot.IsAvailable || newSlot.AvailableSlots <= 0))
            {
                throw new BusinessRuleException(
                    "The selected energy slot is no longer available.");
            }

            // Ensures that the new reservation time falls within the selected slot.
            if (newReservationTime < newSlot.StartTime ||
                newReservationTime > newSlot.EndTime)
            {
                throw new BusinessRuleException(
                    "The reservation time must be within the selected slot.");
            }

            // Updates the reservation with the newly selected booking details.
            reservation.StationId = request.StationId;
            reservation.SlotId = request.SlotId;
            reservation.ReservationTime = newReservationTime;
            reservation.Type = request.Type;
            reservation.EnergyKwh = request.EnergyKwh;
            reservation.UpdatedAt = now;

            // Saves the updated reservation in MongoDB.
            await _db.Reservations.ReplaceOneAsync(
                r => r.Id == reservation.Id,
                reservation);

            // Adjusts slot availability when the reservation has been moved to another slot.
            if (changingSlot && oldSlot != null)
            {
                // Returns the previously reserved capacity to the original slot.
                await _db.Slots.UpdateOneAsync(
                    s => s.Id == oldSlot.Id,
                    Builders<EnergySlot>.Update
                        .Inc(s => s.AvailableSlots, 1)
                        .Set(s => s.IsAvailable, true));

                // Decreases the available capacity of the newly selected slot.
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
            // Retrieves the reservation that is being cancelled.
            var reservation = await _db.Reservations
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            // Ensures that the reservation exists before cancelling it.
            if (reservation == null)
            {
                throw BusinessRuleException.NotFound(
                    "Reservation was not found.");
            }

            // Prevents prosumers from cancelling reservations belonging to other prosumers.
            if (!isStaff && reservation.ProsumerNic != userNic)
            {
                throw BusinessRuleException.Forbidden(
                    "You can only cancel your own reservations.");
            }

            // Prevents an already cancelled reservation from being cancelled again.
            if (reservation.Status == ReservationStatus.Cancelled)
            {
                throw new BusinessRuleException(
                    "This reservation is already cancelled.");
            }

            // Prevents completed reservations from being cancelled.
            if (reservation.Status == ReservationStatus.Completed)
            {
                throw new BusinessRuleException(
                    "A completed reservation cannot be cancelled.");
            }

            // Gets the current UTC time for the twelve-hour cancellation rule.
            var now = DateTime.UtcNow;

            // Ensures that cancellation is requested at least twelve hours before the reservation.
            if (reservation.ReservationTime <= now.AddHours(12))
            {
                throw new BusinessRuleException(
                    "Reservations can only be cancelled at least 12 hours before the reservation time.");
            }

            // Changes the reservation status to Cancelled and updates its modification time.
            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = now;

            // Saves the cancelled reservation in MongoDB.
            await _db.Reservations.ReplaceOneAsync(
                r => r.Id == reservation.Id,
                reservation);

            // Returns the cancelled reservation's capacity to its original slot.
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
            // Retrieves the reservation that is being approved.
            var reservation = await _db.Reservations
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            // Ensures that the reservation exists before approval.
            if (reservation == null)
            {
                throw BusinessRuleException.NotFound(
                    "Reservation was not found.");
            }

            // Ensures that only pending reservations can be approved.
            if (reservation.Status != ReservationStatus.Pending)
            {
                throw new BusinessRuleException(
                    "Only pending reservations can be approved.");
            }

            // Changes the reservation status to Approved and generates a unique QR token.
            reservation.Status = ReservationStatus.Approved;
            reservation.QrToken = Guid.NewGuid().ToString("N");
            reservation.UpdatedAt = DateTime.UtcNow;

            // Saves the approved reservation and generated QR token in MongoDB.
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
            // Ensures that the reservation time is later than the current time.
            if (reservationTime <= now)
            {
                throw new BusinessRuleException(
                    "The reservation time must be in the future.");
            }

            // Ensures that the reservation is not scheduled more than seven days in advance.
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
            // Returns the value directly when it is already stored as UTC.
            if (value.Kind == DateTimeKind.Utc)
                return value;

            // Converts local DateTime values to UTC.
            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();

            // Treats unspecified DateTime values as UTC.
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
            // Retrieves the solar station associated with the reservation.
            var station = await _db.Stations
                .Find(s => s.Id == reservation.StationId)
                .FirstOrDefaultAsync();

            // Maps the reservation data and station name into the API response model.
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