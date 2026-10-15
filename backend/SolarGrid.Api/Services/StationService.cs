/*
 * File:        StationService.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Business logic for managing solar microgrid nodes (stations),
 *              including deactivation validation (R8) and location queries.
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
    /// Station business logic: CRUD, deactivation rule R8 and the nearby search.
    /// </summary>
    public class StationService
    {
        private readonly IMongoCollection<SolarStation> _stations;
        private readonly IMongoCollection<EnergyReservation> _reservations;

        /// <summary>
        /// Gets the station and reservation collections from the database context.
        /// </summary>
        public StationService(MongoDbContext context)
        {
            _stations = context.Stations;
            _reservations = context.Reservations;
        }

        /// <summary>
        /// Returns all stations, or only the active ones when activeOnly is true.
        /// </summary>
        public async Task<List<SolarStation>> GetAllAsync(bool activeOnly = false)
        {
            if (activeOnly)
            {
                return await _stations.Find(s => s.IsActive).ToListAsync();
            }
            return await _stations.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Returns one station, or throws 404 if it does not exist.
        /// </summary>
        public async Task<SolarStation> GetByIdAsync(string id)
        {
            var station = await _stations.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (station == null) throw BusinessRuleException.NotFound("Station not found.");
            return station;
        }

        /// <summary>
        /// Validates and saves a new station. New stations are active straight away.
        /// </summary>
        public async Task<SolarStation> CreateAsync(CreateStationRequest dto)
        {
            // Check GPS, capacity, battery slots and schedule (added by Shermon H)
            MicrogridValidator.ValidateStation(dto);

            var station = new SolarStation
            {
                Name = dto.Name,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CapacityKw = dto.CapacityKw,
                BatterySlots = dto.BatterySlots,
                Schedule = dto.Schedule,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _stations.InsertOneAsync(station);
            return station;
        }

        /// <summary>
        /// Validates and saves changes to an existing station's details and schedule.
        /// </summary>
        public async Task<SolarStation> UpdateAsync(string id, UpdateStationRequest dto)
        {
            var station = await GetByIdAsync(id);

            // Check GPS, capacity, battery slots and schedule (added by Shermon H)
            MicrogridValidator.ValidateStation(dto);

            station.Name = dto.Name;
            station.Address = dto.Address;
            station.Latitude = dto.Latitude;
            station.Longitude = dto.Longitude;
            station.CapacityKw = dto.CapacityKw;
            station.BatterySlots = dto.BatterySlots;
            station.Schedule = dto.Schedule;
            station.UpdatedAt = DateTime.UtcNow;

            await _stations.ReplaceOneAsync(s => s.Id == id, station);
            return station;
        }

        /// <summary>
        /// Rule R8: A node cannot be deactivated while it has Pending or Approved future reservations.
        /// </summary>
        public async Task DeactivateAsync(string id)
        {
            var station = await GetByIdAsync(id);

            var activeReservationsCount = await _reservations.CountDocumentsAsync(r =>
                r.StationId == id &&
                r.ReservationTime > DateTime.UtcNow &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Approved));

            if (activeReservationsCount > 0)
            {
                throw BusinessRuleException.Conflict("Cannot deactivate a station with active future reservations.");
            }

            station.IsActive = false;
            station.UpdatedAt = DateTime.UtcNow;
            await _stations.ReplaceOneAsync(s => s.Id == id, station);
        }

        /// <summary>
        /// Makes a deactivated station active again.
        /// </summary>
        public async Task ActivateAsync(string id)
        {
            var station = await GetByIdAsync(id);
            station.IsActive = true;
            station.UpdatedAt = DateTime.UtcNow;
            await _stations.ReplaceOneAsync(s => s.Id == id, station);
        }

        /// <summary>
        /// Returns the active stations within radiusKm of the given point, nearest first.
        /// The number of stations is small, so all active stations are loaded and the
        /// Haversine distance is checked in memory instead of using a 2dsphere index.
        /// </summary>
        public async Task<List<SolarStation>> GetNearbyAsync(double lat, double lng, double radiusKm)
        {
            if (lat < -90 || lat > 90 || lng < -180 || lng > 180)
            {
                throw new BusinessRuleException("Latitude must be between -90 and 90 and longitude between -180 and 180.");
            }
            if (radiusKm <= 0 || radiusKm > 500)
            {
                throw new BusinessRuleException("Search radius must be between 0 and 500 km.");
            }

            var activeStations = await _stations.Find(s => s.IsActive).ToListAsync();

            return activeStations
                .Select(s => new { Station = s, Distance = CalculateDistance(lat, lng, s.Latitude, s.Longitude) })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Station)
                .ToList();
        }

        /// <summary>
        /// Haversine formula: great-circle distance in km between two GPS points.
        /// Source: https://www.movable-type.co.uk/scripts/latlong.html
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // Earth radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        /// <summary>
        /// Converts an angle from degrees to radians.
        /// </summary>
        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
